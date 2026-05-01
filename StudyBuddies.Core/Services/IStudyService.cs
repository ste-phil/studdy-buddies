namespace StudyBuddies.Core.Services;

public record StudyStats(int DueCount, int NewCount, int LearnedCount);

public interface IStudyService
{
    Task<List<StudyCard>> GetDueCardsAsync(Guid partnershipId, string learnerUserId, int max = 20, int newPerDay = 10, CancellationToken ct = default);

    Task GradeAsync(Guid wordId, string learnerUserId, ReviewGrade grade, CancellationToken ct = default);

    Task<StudyStats> GetStatsAsync(Guid partnershipId, string learnerUserId, CancellationToken ct = default);
}
