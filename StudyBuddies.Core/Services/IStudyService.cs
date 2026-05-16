namespace StudyBuddies.Core.Services;

public record StudyStats(int DueCount, int NewCount, int LearnedCount);

public record TodayAccuracy(int Total, int Correct);

public interface IStudyService
{
    Task<List<StudyCard>> GetDueCardsAsync(Guid partnershipId, string learnerUserId, int max = 20, int newPerDay = 10, CancellationToken ct = default);

    Task GradeAsync(StudyAttemptInput input, string learnerUserId, CancellationToken ct = default);

    Task<StudyStats> GetStatsAsync(Guid partnershipId, string learnerUserId, CancellationToken ct = default);

    Task<int> GetTodayReviewCountAsync(string learnerUserId, CancellationToken ct = default);

    Task<TodayAccuracy> GetTodayAccuracyAsync(string learnerUserId, CancellationToken ct = default);
}
