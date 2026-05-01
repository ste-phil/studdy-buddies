using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StudyBuddies.Core.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Partnership> Partnerships => Set<Partnership>();

    public DbSet<Word> Words => Set<Word>();

    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(x => x.DisplayName).HasMaxLength(100);
            b.Property(x => x.NativeLanguage).HasMaxLength(10);
        });

        builder.Entity<Partnership>(b =>
        {
            b.HasOne(p => p.UserA)
                .WithMany(u => u.PartnershipsAsUserA)
                .HasForeignKey(p => p.UserAId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.UserB)
                .WithMany(u => u.PartnershipsAsUserB)
                .HasForeignKey(p => p.UserBId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.RequestedBy)
                .WithMany()
                .HasForeignKey(p => p.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(p => new { p.UserAId, p.UserBId }).IsUnique();
        });

        builder.Entity<Word>(b =>
        {
            b.HasOne(w => w.Partnership)
                .WithMany(p => p.Words)
                .HasForeignKey(w => w.PartnershipId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(w => w.ForUser)
                .WithMany()
                .HasForeignKey(w => w.ForUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(w => w.ByUser)
                .WithMany()
                .HasForeignKey(w => w.ByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(w => w.Term).HasMaxLength(200);
            b.Property(w => w.Translation).HasMaxLength(200);
            b.Property(w => w.TermLanguage).HasMaxLength(10);
            b.Property(w => w.TranslationLanguage).HasMaxLength(10);
            b.Property(w => w.Example).HasMaxLength(500);
            b.Property(w => w.Notes).HasMaxLength(500);

            var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
                v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v.ToList());

            b.Property(w => w.Tags)
                .HasConversion(
                    v => string.Join('|', v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(stringListComparer);
        });

        builder.Entity<Review>(b =>
        {
            b.HasOne(r => r.Word)
                .WithOne(w => w.Review)
                .HasForeignKey<Review>(r => r.WordId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(r => r.WordId).IsUnique();
        });
    }
}
