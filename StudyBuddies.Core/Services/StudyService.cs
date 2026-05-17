using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public class StudyService(ApplicationDbContext db) : IStudyService
{
    public const int DailyGoal = 10;

    public async Task<List<StudyCard>> GetDueCardsAsync(Guid partnershipId, string learnerUserId, int max = 20, int newPerDay = 10, IReadOnlySet<StudyMode>? allowedModes = null, IReadOnlySet<string>? tagFilter = null, CancellationToken ct = default)
    {
        await EnsureMemberAsync(partnershipId, learnerUserId, ct);

        var today = DateTime.UtcNow.Date;
        var hasTagFilter = tagFilter is { Count: > 0 };

        var dueQuery = db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId
                && w.ForUserId == learnerUserId
                && w.Review != null
                && w.Review.DueDate <= today.AddDays(1))
            .Include(w => w.Review)
            .OrderBy(w => w.Review!.DueDate);

        var dueWords = hasTagFilter
            ? FilterByTags(await dueQuery.ToListAsync(ct), tagFilter!).Take(max).ToList()
            : await dueQuery.Take(max).ToListAsync(ct);

        var newSlots = Math.Max(0, max - dueWords.Count);
        if (newSlots > 0)
        {
            newSlots = Math.Min(newSlots, newPerDay);
        }

        List<Word> newWords;
        if (newSlots > 0)
        {
            var newQuery = db.Words
                .AsNoTracking()
                .Where(w => w.PartnershipId == partnershipId
                    && w.ForUserId == learnerUserId
                    && w.Review == null)
                .OrderBy(w => w.CreatedAt);

            newWords = hasTagFilter
                ? FilterByTags(await newQuery.ToListAsync(ct), tagFilter!).Take(newSlots).ToList()
                : await newQuery.Take(newSlots).ToListAsync(ct);
        }
        else
        {
            newWords = new List<Word>();
        }

        var allCards = dueWords.Concat(newWords).ToList();

        return await BuildCardsAsync(allCards, partnershipId, learnerUserId, allowedModes, ct);
    }

    public async Task<List<StudyCard>> GetExtraPracticeCardsAsync(Guid partnershipId, string learnerUserId, int max = 20, IReadOnlySet<StudyMode>? allowedModes = null, IReadOnlySet<string>? tagFilter = null, CancellationToken ct = default)
    {
        await EnsureMemberAsync(partnershipId, learnerUserId, ct);
        var hasTagFilter = tagFilter is { Count: > 0 };

        var newQuery = db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId
                && w.ForUserId == learnerUserId
                && w.Review == null)
            .OrderBy(w => w.CreatedAt);

        var newWords = hasTagFilter
            ? FilterByTags(await newQuery.ToListAsync(ct), tagFilter!).Take(max).ToList()
            : await newQuery.Take(max).ToListAsync(ct);

        var remaining = Math.Max(0, max - newWords.Count);
        List<Word> reviewedWords;
        if (remaining > 0)
        {
            var reviewedQuery = db.Words
                .AsNoTracking()
                .Where(w => w.PartnershipId == partnershipId
                    && w.ForUserId == learnerUserId
                    && w.Review != null)
                .Include(w => w.Review)
                .OrderBy(w => w.Review!.DueDate);

            reviewedWords = hasTagFilter
                ? FilterByTags(await reviewedQuery.ToListAsync(ct), tagFilter!).Take(remaining).ToList()
                : await reviewedQuery.Take(remaining).ToListAsync(ct);
        }
        else
        {
            reviewedWords = new List<Word>();
        }

        var allWords = newWords.Concat(reviewedWords).ToList();
        return await BuildCardsAsync(allWords, partnershipId, learnerUserId, allowedModes, ct);
    }

    // tagFilter is expected to use a case-insensitive comparer (HashSet<string>(StringComparer.OrdinalIgnoreCase)).
    private static IEnumerable<Word> FilterByTags(IEnumerable<Word> words, IReadOnlySet<string> tagFilter)
    {
        return words.Where(w => w.Tags.Any(t => tagFilter.Contains(t)));
    }

    private async Task<List<StudyCard>> BuildCardsAsync(List<Word> words, Guid partnershipId, string learnerUserId, IReadOnlySet<StudyMode>? allowedModes, CancellationToken ct)
    {
        if (words.Count == 0)
        {
            return new List<StudyCard>();
        }

        var pool = await db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId && w.ForUserId == learnerUserId)
            .Select(w => new { w.Term, w.Translation })
            .ToListAsync(ct);
        var termPool = pool.Select(x => x.Term).Distinct().ToList();
        var translationPool = pool.Select(x => x.Translation).Distinct().ToList();

        var rng = Random.Shared;
        return words.Select(w =>
        {
            var forwardDistractorCount = translationPool.Count(t => t != w.Translation);
            var mode = StudyModeSelector.Pick(w, w.Review, forwardDistractorCount, allowedModes);

            // Cloze stays forward (Example is in TermLanguage). Other modes flip 50/50.
            var reversed = mode != StudyMode.Cloze && rng.Next(2) == 0;

            var promptText = reversed ? w.Translation : w.Term;
            var promptLang = reversed ? w.TranslationLanguage : w.TermLanguage;
            var answerText = reversed ? w.Term : w.Translation;
            var answerLang = reversed ? w.TermLanguage : w.TranslationLanguage;
            var sourcePool = reversed ? termPool : translationPool;

            IReadOnlyList<string>? distractors = null;
            if (mode == StudyMode.MultipleChoice)
            {
                distractors = sourcePool
                    .Where(t => t != answerText)
                    .OrderBy(_ => rng.Next())
                    .Take(3)
                    .ToList();

                // If reversal left too few distractors, fall back to forward.
                if (distractors.Count < 3 && reversed)
                {
                    distractors = translationPool
                        .Where(t => t != w.Translation)
                        .OrderBy(_ => rng.Next())
                        .Take(3)
                        .ToList();
                    promptText = w.Term;
                    promptLang = w.TermLanguage;
                    answerText = w.Translation;
                    answerLang = w.TranslationLanguage;
                }
            }

            return new StudyCard(
                w.Id,
                promptText,
                promptLang,
                answerText,
                answerLang,
                w.Example,
                w.Notes,
                w.Tags,
                mode,
                distractors);
        }).ToList();
    }

    public async Task GradeAsync(StudyAttemptInput input, string learnerUserId, CancellationToken ct = default)
    {
        var word = await db.Words
            .Include(w => w.Review)
            .FirstOrDefaultAsync(w => w.Id == input.WordId, ct)
            ?? throw new InvalidOperationException("Word not found.");

        if (word.ForUserId != learnerUserId)
        {
            throw new InvalidOperationException("This word is not assigned to you.");
        }

        var review = word.Review ?? new Review { WordId = word.Id };
        var isNew = word.Review is null;

        var now = DateTime.UtcNow;
        Sm2Service.Apply(review, input.Grade, now);

        if (isNew)
        {
            db.Reviews.Add(review);
        }

        db.ReviewAttempts.Add(new ReviewAttempt
        {
            WordId = word.Id,
            UserId = learnerUserId,
            Mode = input.Mode,
            Grade = input.Grade,
            IsCorrect = input.IsCorrect,
            UserAnswer = input.UserAnswer,
            Confidence = input.Confidence,
            AnsweredAt = now,
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<StudyStats> GetStatsAsync(Guid partnershipId, string learnerUserId, CancellationToken ct = default)
    {
        await EnsureMemberAsync(partnershipId, learnerUserId, ct);

        var today = DateTime.UtcNow.Date.AddDays(1);

        var due = await db.Words.CountAsync(w =>
            w.PartnershipId == partnershipId
            && w.ForUserId == learnerUserId
            && w.Review != null
            && w.Review.DueDate <= today, ct);

        var fresh = await db.Words.CountAsync(w =>
            w.PartnershipId == partnershipId
            && w.ForUserId == learnerUserId
            && w.Review == null, ct);

        var learned = await db.Words.CountAsync(w =>
            w.PartnershipId == partnershipId
            && w.ForUserId == learnerUserId
            && w.Review != null
            && w.Review.Repetitions >= 3, ct);

        return new StudyStats(due, fresh, learned);
    }

    public async Task<int> GetTodayReviewCountAsync(string learnerUserId, CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        return await db.Reviews
            .AsNoTracking()
            .Where(r => r.Word.ForUserId == learnerUserId
                && r.LastReview != null
                && r.LastReview >= todayUtc)
            .CountAsync(ct);
    }

    public async Task<TodayAccuracy> GetTodayAccuracyAsync(string learnerUserId, CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var rows = await db.ReviewAttempts
            .AsNoTracking()
            .Where(a => a.UserId == learnerUserId && a.AnsweredAt >= todayUtc)
            .Select(a => a.IsCorrect)
            .ToListAsync(ct);

        return new TodayAccuracy(rows.Count, rows.Count(c => c));
    }

    private async Task EnsureMemberAsync(Guid partnershipId, string userId, CancellationToken ct)
    {
        var exists = await db.Partnerships.AnyAsync(p =>
            p.Id == partnershipId
            && (p.UserAId == userId || p.UserBId == userId), ct);

        if (!exists)
        {
            throw new InvalidOperationException("You are not part of this partnership.");
        }
    }
}
