using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Referral : BaseAuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReferrerUserId { get; set; }
        public string ReferralCode { get; set; } = default!; 
        public ReferralStatus Status { get; set; } = ReferralStatus.Pending;        
        public DateTimeOffset? AcceptedAtUtc { get; set; }
        public Guid? AcceptedByUserId { get; set; }
        public string? Link { get; set; }
        public string Token{ get; set; }
        public string Slug { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? TokenConsumedUtc { get; set; }
        public string? Campaign { get; set; }

        public static Referral Create(Guid userId, string referralCode, string? campaign) => new()
        {
            ReferrerUserId = userId,
            ReferralCode = referralCode,
            Campaign = campaign
        };
    }
}
