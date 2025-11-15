namespace api.Contracts
{
    public class ReferralValidationResponse
    {
        public bool Validated { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
