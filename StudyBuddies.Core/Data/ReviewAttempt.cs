using StudyBuddies.Core.Services;

namespace StudyBuddies.Core.Data;

public class ReviewAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WordId { get; set; }

    public Word Word { get; set; } = null!;

    public string UserId { get; set; } = "";

    public StudyMode Mode { get; set; }

    public ReviewGrade Grade { get; set; }

    public bool IsCorrect { get; set; }

    public string? UserAnswer { get; set; }

    public byte? Confidence { get; set; }

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}
