using Domain.Entities;
using Domain.Enums;

namespace api.Contracts
{
    public class ReferralResponse
    {
        public Guid Id { get; set; }
        public Guid ReferrerUserId { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
        public ReferralStatus Status { get; set; }
        public string? Link { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; 
        public string? Campaign { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset? ReservedAtUtc { get; set; }
        public DateTimeOffset? AcceptedAtUtc { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? TokenConsumedUtc { get; set; }

        public static ReferralResponse FromEntity(Referral referral) =>
            new()
            {
                Id = referral.Id,
                ReferrerUserId = referral.ReferrerUserId,
                ReferralCode = referral.ReferralCode,
                Status = referral.Status,
                Link = referral.Link,
                Slug = referral.Slug,
                Token = referral.Token, 
                Campaign = referral.Campaign,
                CreatedAtUtc = referral.CreatedAtUtc, 
                ReservedAtUtc = referral.ReservedAtUtc,
                AcceptedAtUtc = referral.AcceptedAtUtc, 
                ExpiresAt = referral.ExpiresAt,
                TokenConsumedUtc = referral.TokenConsumedUtc,

            };
    }
}
