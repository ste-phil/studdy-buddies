namespace StudyBuddies.Core.Data;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WordId { get; set; }

    public Word Word { get; set; } = null!;

    public double EaseFactor { get; set; } = 2.5;

    public int Interval { get; set; }

    public int Repetitions { get; set; }

    public DateTime DueDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastReview { get; set; }
}
