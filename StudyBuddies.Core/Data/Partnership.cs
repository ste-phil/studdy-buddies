namespace StudyBuddies.Core.Data;

public enum PartnershipStatus
{
    Pending,
    Active,
    Declined,
    Ended
}

public class Partnership
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserAId { get; set; } = "";

    public ApplicationUser UserA { get; set; } = null!;

    public string UserBId { get; set; } = "";

    public ApplicationUser UserB { get; set; } = null!;

    public string RequestedById { get; set; } = "";

    public ApplicationUser RequestedBy { get; set; } = null!;

    public PartnershipStatus Status { get; set; } = PartnershipStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AcceptedAt { get; set; }

    public ICollection<Word> Words { get; set; } = new List<Word>();
}
