using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public class StudyService(ApplicationDbContext db) : IStudyService
{
    public const int DailyGoal = 10;

    public async Task<List<StudyCard>> GetDueCardsAsync(Guid partnershipId, string learnerUserId, int max = 20, int newPerDay = 10, CancellationToken ct = default)
    {
        await EnsureMemberAsync(partnershipId, learnerUserId, ct);

        var today = DateTime.UtcNow.Date;

        var dueWords = await db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId
                && w.ForUserId == learnerUserId
                && w.Review != null
                && w.Review.DueDate <= today.AddDays(1))
            .Include(w => w.Review)
            .OrderBy(w => w.Review!.DueDate)
            .Take(max)
            .ToListAsync(ct);

        var newSlots = Math.Max(0, max - dueWords.Count);
        if (newSlots > 0)
        {
            newSlots = Math.Min(newSlots, newPerDay);
        }

        var newWords = newSlots > 0
            ? await db.Words
                .AsNoTracking()
                .Where(w => w.PartnershipId == partnershipId
                    && w.ForUserId == learnerUserId
                    && w.Review == null)
                .OrderBy(w => w.CreatedAt)
                .Take(newSlots)
                .ToListAsync(ct)
            : new List<Word>();

        var allCards = dueWords.Concat(newWords).ToList();

        var translationPool = await db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId && w.ForUserId == learnerUserId)
            .Select(w => w.Translation)
            .Distinct()
            .ToListAsync(ct);

        var rng = Random.Shared;
        return allCards.Select(w =>
        {
            var distractorCount = translationPool.Count(t => t != w.Translation);
            var mode = StudyModeSelector.Pick(w, w.Review, distractorCount);

            IReadOnlyList<string>? distractors = null;
            if (mode == StudyMode.MultipleChoice)
            {
                distractors = translationPool
                    .Where(t => t != w.Translation)
                    .OrderBy(_ => rng.Next())
                    .Take(3)
                    .ToList();
            }

            return new StudyCard(
                w.Id,
                w.Term,
                w.TermLanguage,
                w.Translation,
                w.TranslationLanguage,
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
