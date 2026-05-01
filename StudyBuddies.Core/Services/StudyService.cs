using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public class StudyService(ApplicationDbContext db) : IStudyService
{
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

        return dueWords.Concat(newWords)
            .Select(w => new StudyCard(
                w.Id,
                w.Term,
                w.TermLanguage,
                w.Translation,
                w.TranslationLanguage,
                w.Example,
                w.Notes))
            .ToList();
    }

    public async Task GradeAsync(Guid wordId, string learnerUserId, ReviewGrade grade, CancellationToken ct = default)
    {
        var word = await db.Words
            .Include(w => w.Review)
            .FirstOrDefaultAsync(w => w.Id == wordId, ct)
            ?? throw new InvalidOperationException("Word not found.");

        if (word.ForUserId != learnerUserId)
        {
            throw new InvalidOperationException("This word is not assigned to you.");
        }

        var review = word.Review ?? new Review { WordId = word.Id };
        var isNew = word.Review is null;

        Sm2Service.Apply(review, grade, DateTime.UtcNow);

        if (isNew)
        {
            db.Reviews.Add(review);
        }

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
