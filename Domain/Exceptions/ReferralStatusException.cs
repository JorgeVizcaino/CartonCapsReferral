using System.Net;

namespace Domain.Exceptions
{
    public sealed class ReferralStatusException : DomainException
    {
        public ReferralStatusException(string message)
            : base(message)
        {
        }

        public override string ErrorCode => "referral.invalid_status";

        public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    }
}
