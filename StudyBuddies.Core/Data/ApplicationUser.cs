using Microsoft.AspNetCore.Identity;

namespace StudyBuddies.Core.Data;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "";

    public string NativeLanguage { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? LastUsedPartnershipId { get; set; }

    public ICollection<Partnership> PartnershipsAsUserA { get; set; } = new List<Partnership>();

    public ICollection<Partnership> PartnershipsAsUserB { get; set; } = new List<Partnership>();
}
