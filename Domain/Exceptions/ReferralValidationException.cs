using System.Net;

namespace Domain.Exceptions
{
    public sealed class ReferralValidationException : DomainException
    {
        public ReferralValidationException(string message)
            : base(message)
        {
        }

        public override string ErrorCode => "referral.validation_failed";
    }
}
