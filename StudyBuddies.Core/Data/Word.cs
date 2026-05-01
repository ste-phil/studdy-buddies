namespace StudyBuddies.Core.Data;

public class Word
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PartnershipId { get; set; }

    public Partnership Partnership { get; set; } = null!;

    public string ForUserId { get; set; } = "";

    public ApplicationUser ForUser { get; set; } = null!;

    public string ByUserId { get; set; } = "";

    public ApplicationUser ByUser { get; set; } = null!;

    public string Term { get; set; } = "";

    public string TermLanguage { get; set; } = "";

    public string Translation { get; set; } = "";

    public string TranslationLanguage { get; set; } = "";

    public string? Example { get; set; }

    public string? Notes { get; set; }

    public List<string> Tags { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
}
