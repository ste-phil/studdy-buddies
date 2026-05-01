using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public interface IPartnershipService
{
    Task<List<UserSummary>> SearchUsersAsync(string query, string currentUserId, CancellationToken ct = default);

    Task<Guid> RequestPartnershipAsync(string fromUserId, string toUserId, CancellationToken ct = default);

    Task AcceptPartnershipAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default);

    Task DeclinePartnershipAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default);

    Task<List<PartnershipSummary>> GetPartnershipsAsync(string userId, CancellationToken ct = default);

    Task<PartnershipSummary?> GetPartnershipAsync(Guid partnershipId, string currentUserId, CancellationToken ct = default);
}
