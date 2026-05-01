namespace StudyBuddies.Core.Services;

public interface IWordService
{
    Task<Guid> CreateWordAsync(Guid partnershipId, string byUserId, WordCreateInput input, CancellationToken ct = default);

    Task<List<WordSummary>> ListWordsAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default);

    Task<WordSummary?> GetWordAsync(Guid wordId, string currentUserId, CancellationToken ct = default);

    Task UpdateWordAsync(Guid wordId, string currentUserId, WordCreateInput input, CancellationToken ct = default);

    Task DeleteWordAsync(Guid wordId, string currentUserId, CancellationToken ct = default);

    Task<List<string>> GetExistingTagsAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default);
}
