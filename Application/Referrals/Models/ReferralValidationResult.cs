namespace Application.Referrals.Models
{
    public class ReferralValidationResult
    {
        public bool Validated { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
