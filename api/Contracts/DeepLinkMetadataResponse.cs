using Domain.Entities;
using Domain.Enums;

namespace api.Contracts
{
    public class DeepLinkMetadataResponse
    {
        public Guid Id { get; set; } 
        public string Slug { get; set; } = string.Empty;
        public string? Link { get; set; }
        public ReferralStatus Status { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
        public Guid ReferrerUserId { get; set; }
        public string? Campaign { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }

        public static DeepLinkMetadataResponse FromEntity(Referral referral) =>
            new()
            {
                Id = referral.Id,
                Slug = referral.Slug, 
                Link = referral.Link,
                Status = referral.Status,
                ReferralCode = referral.ReferralCode, 
                ReferrerUserId = referral.ReferrerUserId,
                Campaign = referral.Campaign,
                ExpiresAt = referral.ExpiresAt,

            };
    }
}
