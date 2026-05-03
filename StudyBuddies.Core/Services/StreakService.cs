using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public class StreakService(ApplicationDbContext db) : IStreakService
{
    public async Task<int> GetCurrentStreakAsync(string userId, CancellationToken ct = default)
    {
        var dates = await db.Reviews
            .AsNoTracking()
            .Where(r => r.Word.ForUserId == userId && r.LastReview != null)
            .Select(r => r.LastReview!.Value)
            .ToListAsync(ct);

        if (dates.Count == 0) return 0;

        var distinctDays = dates
            .Select(d => DateOnly.FromDateTime(d.ToUniversalTime()))
            .Distinct()
            .ToHashSet();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cursor = distinctDays.Contains(today) ? today : today.AddDays(-1);

        if (!distinctDays.Contains(cursor)) return 0;

        var streak = 0;
        while (distinctDays.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }
}
