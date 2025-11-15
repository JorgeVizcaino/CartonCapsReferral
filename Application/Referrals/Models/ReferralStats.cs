using Domain.Enums;

namespace Application.Referrals.Models
{
    public class ReferralStats
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Reserved { get; set; }
        public int Accepted { get; set; }
        public int Expired { get; set; }
        public int Cancelled { get; set; }

        public static ReferralStats FromCounts(IReadOnlyDictionary<ReferralStatus, int> counts)
        {
            // Dictionary helper to avoid TryGetValue calls for missing status.
            int Get(ReferralStatus status) => counts.GetValueOrDefault(status, 0);

            return new ReferralStats
            {
                Total = counts.Values.Sum(),
                Pending = Get(ReferralStatus.Pending),
                Reserved = Get(ReferralStatus.Reserved),
                Accepted = Get(ReferralStatus.Accepted),
                Expired = Get(ReferralStatus.Expired),
                Cancelled = Get(ReferralStatus.Cancelled)
            };
        }
    }
}
