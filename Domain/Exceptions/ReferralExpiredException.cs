using System;
using System.Net;

namespace Domain.Exceptions
{
    public sealed class ReferralExpiredException : DomainException
    {
        public ReferralExpiredException(Guid referralId)
            : base($"Referral '{referralId}' expired and can no longer be used.")
        {
        }

        public override string ErrorCode => "referral.expired";

        public override HttpStatusCode StatusCode => HttpStatusCode.Gone;
    }
}
