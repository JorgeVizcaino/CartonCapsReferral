using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class Referral : BaseAuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReferrerUserId { get; set; }
        public string ReferralCode { get; set; } = default!;
        public ReferralStatus Status { get; private set; } = ReferralStatus.Pending;
        public DateTimeOffset? AcceptedAtUtc { get; set; }
        public Guid? AcceptedByUserId { get; set; }
        public string? Link { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? TokenConsumedUtc { get; set; }
        public string? Campaign { get; set; }
        public string? IdempotencyKey { get; set; }
        public DateTimeOffset? ReservedAtUtc { get; private set; }

        public static Referral Create(Guid userId, string referralCode, string? campaign, string? idempotencyKey) => new()
        {
            ReferrerUserId = userId,
            ReferralCode = referralCode,
            Campaign = campaign,
            IdempotencyKey = idempotencyKey
        };

        public void Reserve(DateTimeOffset reservedAtUtc)
        {
            if (Status == ReferralStatus.Reserved)
            {
                return;
            }

            if (Status != ReferralStatus.Pending)
            {
                throw new ReferralStatusException($"Referral '{Id}' cannot be reserved from status '{Status}'.");
            }

            Status = ReferralStatus.Reserved;
            ReservedAtUtc = reservedAtUtc;
        }

        public void Accept(DateTimeOffset acceptedAtUtc, Guid? acceptedByUserId = null)
        {
            if (Status == ReferralStatus.Accepted)
            {
                return;
            }

            if (Status != ReferralStatus.Pending && Status != ReferralStatus.Reserved)
            {
                throw new ReferralStatusException($"Referral '{Id}' cannot be accepted from status '{Status}'.");
            }

            if (ExpiresAt.HasValue && ExpiresAt.Value <= acceptedAtUtc)
            {
                throw new ReferralExpiredException(Id);
            }

            Status = ReferralStatus.Accepted;
            AcceptedAtUtc = acceptedAtUtc;
            AcceptedByUserId = acceptedByUserId;
            TokenConsumedUtc = acceptedAtUtc;
        }

        public void Cancel(string reason)
        {
            if (Status == ReferralStatus.Cancelled)
            {
                return;
            }

            if (Status == ReferralStatus.Accepted)
            {
                throw new ReferralStatusException($"Accepted referral '{Id}' cannot be cancelled. Reason: {reason}");
            }

            Status = ReferralStatus.Cancelled;
        }
    }
}
