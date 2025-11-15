namespace api.Contracts
{
    public class AcceptReferralResponse
    {
        public Guid ReferralId { get; set; } 
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? TokenConsumedUtc { get; set; } 
    }
}
