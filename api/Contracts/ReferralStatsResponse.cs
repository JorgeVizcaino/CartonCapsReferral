using Application.Referrals.Models;

namespace api.Contracts
{
    public class ReferralStatsResponse
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Reserved { get; set; }
        public int Accepted { get; set; }
        public int Expired { get; set; }
        public int Cancelled { get; set; }

        public static ReferralStatsResponse FromModel(ReferralStats stats) =>
            new()
            {
                Total = stats.Total,
                Pending = stats.Pending,
                Reserved = stats.Reserved,
                Accepted = stats.Accepted,
                Expired = stats.Expired,
                Cancelled = stats.Cancelled, 

            };
    }
}
