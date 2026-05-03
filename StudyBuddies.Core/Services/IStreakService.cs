namespace StudyBuddies.Core.Services;

public interface IStreakService
{
    Task<int> GetCurrentStreakAsync(string userId, CancellationToken ct = default);
}
