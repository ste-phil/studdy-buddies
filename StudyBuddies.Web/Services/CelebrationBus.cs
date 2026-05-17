namespace StudyBuddies.Web.Services;

public enum CelebrationKind
{
    DailyGoal,
    SessionComplete,
    BadgeUnlocked,
}

public record CelebrationPayload(
    CelebrationKind Kind,
    string Emoji,
    string Title,
    string? Subtitle = null);

public sealed class CelebrationBus
{
    public event Func<CelebrationPayload, Task>? OnCelebrate;

    public async Task FireAsync(CelebrationPayload payload)
    {
        var handler = OnCelebrate;
        if (handler is null) return;
        foreach (var d in handler.GetInvocationList().Cast<Func<CelebrationPayload, Task>>())
        {
            try { await d(payload); }
            catch { /* swallow; UI bus must not break a study session */ }
        }
    }
}
