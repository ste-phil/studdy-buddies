using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public record UserSummary(string Id, string DisplayName, string Email, string NativeLanguage);

public record PartnershipSummary(
    Guid Id,
    UserSummary Partner,
    PartnershipStatus Status,
    bool IsIncomingRequest,
    DateTime CreatedAt,
    DateTime? AcceptedAt);

public record WordCreateInput(
    string Term,
    string Translation,
    string? Example,
    string? Notes,
    List<string>? Tags);

public record WordSummary(
    Guid Id,
    string Term,
    string TermLanguage,
    string Translation,
    string TranslationLanguage,
    string? Example,
    string? Notes,
    List<string> Tags,
    string ByUserId,
    string ForUserId,
    bool IsAuthoredByMe,
    DateTime CreatedAt,
    DateTime? DueDate,
    Guid PartnershipId = default,
    string? PartnerDisplayName = null,
    string? PartnerLanguage = null);

public enum StudyMode
{
    Flashcard = 0,
    Type = 1,
    MultipleChoice = 2,
    Listening = 3,
    Cloze = 4,
}

public record StudyCard(
    Guid WordId,
    string Term,
    string TermLanguage,
    string Translation,
    string TranslationLanguage,
    string? Example,
    string? Notes,
    IReadOnlyList<string> Tags,
    StudyMode Mode,
    IReadOnlyList<string>? Distractors);

public record StudyAttemptInput(
    Guid WordId,
    StudyMode Mode,
    ReviewGrade Grade,
    bool IsCorrect,
    string? UserAnswer,
    byte? Confidence);

public record DashboardStats(
    int DueCount,
    int NewCount,
    int LearnedCount,
    int Streak,
    int TodayCount,
    int DailyGoal);
