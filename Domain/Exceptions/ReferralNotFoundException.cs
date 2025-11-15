using System;
using System.Net;

namespace Domain.Exceptions
{
    public sealed class ReferralNotFoundException : DomainException
    {
        public ReferralNotFoundException(string reason)
            : base(reason)
        {
        }

        public override string ErrorCode => "referral.not_found";

        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    }
}
