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
    DateTime? DueDate);

public record StudyCard(
    Guid WordId,
    string Term,
    string TermLanguage,
    string Translation,
    string TranslationLanguage,
    string? Example,
    string? Notes);
