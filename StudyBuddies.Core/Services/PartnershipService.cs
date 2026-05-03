using Microsoft.EntityFrameworkCore;
using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public class PartnershipService(ApplicationDbContext db) : IPartnershipService
{
    public async Task<List<UserSummary>> SearchUsersAsync(string query, string currentUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<UserSummary>();
        }

        var q = query.Trim().ToLower();

        var connectedUserIds = await db.Partnerships
            .Where(p => (p.UserAId == currentUserId || p.UserBId == currentUserId)
                && (p.Status == PartnershipStatus.Active || p.Status == PartnershipStatus.Pending))
            .Select(p => p.UserAId == currentUserId ? p.UserBId : p.UserAId)
            .ToListAsync(ct);

        var excluded = new HashSet<string>(connectedUserIds) { currentUserId };

        return await db.Users
            .Where(u => !excluded.Contains(u.Id))
            .Where(u =>
                (u.DisplayName != null && u.DisplayName.ToLower().Contains(q)) ||
                (u.Email != null && u.Email.ToLower().Contains(q)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(q)))
            .OrderBy(u => u.DisplayName)
            .Take(20)
            .Select(u => new UserSummary(u.Id, u.DisplayName, u.Email ?? "", u.NativeLanguage))
            .ToListAsync(ct);
    }

    public async Task<Guid> RequestPartnershipAsync(string fromUserId, string toUserId, CancellationToken ct = default)
    {
        if (fromUserId == toUserId)
        {
            throw new InvalidOperationException("Cannot send partnership request to yourself.");
        }

        var (userAId, userBId) = SortIds(fromUserId, toUserId);

        var existing = await db.Partnerships
            .FirstOrDefaultAsync(p => p.UserAId == userAId && p.UserBId == userBId, ct);

        if (existing != null)
        {
            if (existing.Status == PartnershipStatus.Active || existing.Status == PartnershipStatus.Pending)
            {
                throw new InvalidOperationException("A partnership or pending request already exists.");
            }

            existing.Status = PartnershipStatus.Pending;
            existing.RequestedById = fromUserId;
            existing.CreatedAt = DateTime.UtcNow;
            existing.AcceptedAt = null;
            await db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var partnership = new Partnership
        {
            UserAId = userAId,
            UserBId = userBId,
            RequestedById = fromUserId,
            Status = PartnershipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.Partnerships.Add(partnership);
        await db.SaveChangesAsync(ct);
        return partnership.Id;
    }

    public async Task AcceptPartnershipAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default)
    {
        var partnership = await db.Partnerships
            .FirstOrDefaultAsync(p => p.Id == partnershipId, ct)
            ?? throw new InvalidOperationException("Partnership not found.");

        if (partnership.Status != PartnershipStatus.Pending)
        {
            throw new InvalidOperationException("Partnership is not pending.");
        }

        if (partnership.RequestedById == currentUserId)
        {
            throw new InvalidOperationException("Only the recipient can accept the request.");
        }

        if (partnership.UserAId != currentUserId && partnership.UserBId != currentUserId)
        {
            throw new InvalidOperationException("You are not part of this partnership.");
        }

        partnership.Status = PartnershipStatus.Active;
        partnership.AcceptedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeclinePartnershipAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default)
    {
        var partnership = await db.Partnerships
            .FirstOrDefaultAsync(p => p.Id == partnershipId, ct)
            ?? throw new InvalidOperationException("Partnership not found.");

        if (partnership.UserAId != currentUserId && partnership.UserBId != currentUserId)
        {
            throw new InvalidOperationException("You are not part of this partnership.");
        }

        partnership.Status = PartnershipStatus.Declined;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<PartnershipSummary>> GetPartnershipsAsync(string userId, CancellationToken ct = default)
    {
        var rows = await db.Partnerships
            .AsNoTracking()
            .Where(p => (p.UserAId == userId || p.UserBId == userId)
                && (p.Status == PartnershipStatus.Active || p.Status == PartnershipStatus.Pending))
            .Include(p => p.UserA)
            .Include(p => p.UserB)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return rows.Select(p =>
        {
            var partner = p.UserAId == userId ? p.UserB : p.UserA;
            var isIncoming = p.Status == PartnershipStatus.Pending && p.RequestedById != userId;
            return new PartnershipSummary(
                p.Id,
                new UserSummary(partner.Id, partner.DisplayName, partner.Email ?? "", partner.NativeLanguage),
                p.Status,
                isIncoming,
                p.CreatedAt,
                p.AcceptedAt);
        }).ToList();
    }

    public async Task<PartnershipSummary?> GetPartnershipAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default)
    {
        var p = await db.Partnerships
            .AsNoTracking()
            .Include(x => x.UserA)
            .Include(x => x.UserB)
            .FirstOrDefaultAsync(x => x.Id == partnershipId, ct);

        if (p is null)
        {
            return null;
        }

        if (p.UserAId != currentUserId && p.UserBId != currentUserId)
        {
            return null;
        }

        var partner = p.UserAId == currentUserId ? p.UserB : p.UserA;
        var isIncoming = p.Status == PartnershipStatus.Pending && p.RequestedById != currentUserId;

        return new PartnershipSummary(
            p.Id,
            new UserSummary(partner.Id, partner.DisplayName, partner.Email ?? "", partner.NativeLanguage),
            p.Status,
            isIncoming,
            p.CreatedAt,
            p.AcceptedAt);
    }

    public async Task<PartnershipSummary?> GetCurrentPartnershipAsync(string userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return null;
        }

        var activePartnerships = await db.Partnerships
            .AsNoTracking()
            .Where(p => (p.UserAId == userId || p.UserBId == userId) && p.Status == PartnershipStatus.Active)
            .Include(p => p.UserA)
            .Include(p => p.UserB)
            .ToListAsync(ct);

        if (activePartnerships.Count == 0)
        {
            return null;
        }

        var preferred = user.LastUsedPartnershipId is { } lastId
            ? activePartnerships.FirstOrDefault(p => p.Id == lastId)
            : null;

        var chosen = preferred ?? activePartnerships
            .OrderByDescending(p => p.AcceptedAt ?? p.CreatedAt)
            .First();

        var partner = chosen.UserAId == userId ? chosen.UserB : chosen.UserA;
        return new PartnershipSummary(
            chosen.Id,
            new UserSummary(partner.Id, partner.DisplayName, partner.Email ?? "", partner.NativeLanguage),
            chosen.Status,
            false,
            chosen.CreatedAt,
            chosen.AcceptedAt);
    }

    public async Task SetLastUsedPartnershipAsync(string userId, Guid partnershipId, CancellationToken ct = default)
    {
        var partnership = await db.Partnerships
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == partnershipId, ct);

        if (partnership is null
            || partnership.Status != PartnershipStatus.Active
            || (partnership.UserAId != userId && partnership.UserBId != userId))
        {
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return;
        }

        user.LastUsedPartnershipId = partnershipId;
        await db.SaveChangesAsync(ct);
    }

    public Task<int> GetIncomingRequestCountAsync(string userId, CancellationToken ct = default)
        => db.Partnerships
            .AsNoTracking()
            .CountAsync(p => (p.UserAId == userId || p.UserBId == userId)
                && p.Status == PartnershipStatus.Pending
                && p.RequestedById != userId, ct);

    private static (string userAId, string userBId) SortIds(string a, string b)
        => string.CompareOrdinal(a, b) < 0 ? (a, b) : (b, a);
}
