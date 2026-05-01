using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public class WordService(ApplicationDbContext db) : IWordService
{
    public async Task<Guid> CreateWordAsync(Guid partnershipId, string byUserId, WordCreateInput input, CancellationToken ct = default)
    {
        ValidateInput(input);

        var partnership = await db.Partnerships
            .Include(p => p.UserA)
            .Include(p => p.UserB)
            .FirstOrDefaultAsync(p => p.Id == partnershipId, ct)
            ?? throw new InvalidOperationException("Partnership not found.");

        if (partnership.Status != PartnershipStatus.Active)
        {
            throw new InvalidOperationException("Partnership is not active.");
        }

        var (byUser, forUser) = ResolveMembers(partnership, byUserId);

        var word = new Word
        {
            PartnershipId = partnership.Id,
            ByUserId = byUser.Id,
            ForUserId = forUser.Id,
            Term = input.Term.Trim(),
            TermLanguage = byUser.NativeLanguage,
            Translation = input.Translation.Trim(),
            TranslationLanguage = forUser.NativeLanguage,
            Example = string.IsNullOrWhiteSpace(input.Example) ? null : input.Example.Trim(),
            Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
            Tags = NormalizeTags(input.Tags),
            CreatedAt = DateTime.UtcNow
        };

        db.Words.Add(word);
        await db.SaveChangesAsync(ct);
        return word.Id;
    }

    public async Task<List<WordSummary>> ListWordsAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default)
    {
        await EnsureMemberAsync(partnershipId, currentUserId, ct);

        return await db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId)
            .Include(w => w.Review)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WordSummary(
                w.Id,
                w.Term,
                w.TermLanguage,
                w.Translation,
                w.TranslationLanguage,
                w.Example,
                w.Notes,
                w.Tags,
                w.ByUserId,
                w.ForUserId,
                w.ByUserId == currentUserId,
                w.CreatedAt,
                w.Review != null ? w.Review.DueDate : (DateTime?)null))
            .ToListAsync(ct);
    }

    public async Task<WordSummary?> GetWordAsync(Guid wordId, string currentUserId, CancellationToken ct = default)
    {
        var w = await db.Words
            .AsNoTracking()
            .Include(x => x.Partnership)
            .Include(x => x.Review)
            .FirstOrDefaultAsync(x => x.Id == wordId, ct);

        if (w is null)
        {
            return null;
        }

        if (w.Partnership.UserAId != currentUserId && w.Partnership.UserBId != currentUserId)
        {
            return null;
        }

        return new WordSummary(
            w.Id,
            w.Term,
            w.TermLanguage,
            w.Translation,
            w.TranslationLanguage,
            w.Example,
            w.Notes,
            w.Tags,
            w.ByUserId,
            w.ForUserId,
            w.ByUserId == currentUserId,
            w.CreatedAt,
            w.Review?.DueDate);
    }

    public async Task UpdateWordAsync(Guid wordId, string currentUserId, WordCreateInput input, CancellationToken ct = default)
    {
        ValidateInput(input);

        var word = await db.Words.FirstOrDefaultAsync(w => w.Id == wordId, ct)
            ?? throw new InvalidOperationException("Word not found.");

        if (word.ByUserId != currentUserId)
        {
            throw new InvalidOperationException("Only the author can edit this word.");
        }

        word.Term = input.Term.Trim();
        word.Translation = input.Translation.Trim();
        word.Example = string.IsNullOrWhiteSpace(input.Example) ? null : input.Example.Trim();
        word.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
        word.Tags = NormalizeTags(input.Tags);

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteWordAsync(Guid wordId, string currentUserId, CancellationToken ct = default)
    {
        var word = await db.Words.FirstOrDefaultAsync(w => w.Id == wordId, ct)
            ?? throw new InvalidOperationException("Word not found.");

        if (word.ByUserId != currentUserId)
        {
            throw new InvalidOperationException("Only the author can delete this word.");
        }

        db.Words.Remove(word);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<string>> GetExistingTagsAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default)
    {
        await EnsureMemberAsync(partnershipId, currentUserId, ct);

        var tagLists = await db.Words
            .AsNoTracking()
            .Where(w => w.PartnershipId == partnershipId)
            .Select(w => w.Tags)
            .ToListAsync(ct);

        return tagLists
            .SelectMany(t => t)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    private async Task<Partnership> EnsureMemberAsync(Guid partnershipId, string currentUserId, CancellationToken ct)
    {
        var partnership = await db.Partnerships
            .FirstOrDefaultAsync(p => p.Id == partnershipId, ct)
            ?? throw new InvalidOperationException("Partnership not found.");

        if (partnership.UserAId != currentUserId && partnership.UserBId != currentUserId)
        {
            throw new InvalidOperationException("You are not part of this partnership.");
        }

        return partnership;
    }

    private static (ApplicationUser byUser, ApplicationUser forUser) ResolveMembers(Partnership partnership, string byUserId)
    {
        if (partnership.UserAId == byUserId)
        {
            return (partnership.UserA, partnership.UserB);
        }

        if (partnership.UserBId == byUserId)
        {
            return (partnership.UserB, partnership.UserA);
        }

        throw new InvalidOperationException("You are not part of this partnership.");
    }

    private static void ValidateInput(WordCreateInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Term))
        {
            throw new InvalidOperationException("Term is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Translation))
        {
            throw new InvalidOperationException("Translation is required.");
        }
    }

    private static List<string> NormalizeTags(List<string>? tags)
    {
        if (tags is null)
        {
            return new List<string>();
        }

        return tags
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
