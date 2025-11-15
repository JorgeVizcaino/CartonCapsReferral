using System;
using System.Net;

namespace Domain.Exceptions
{
    public sealed class RateLimitExceededException : DomainException
    {
        public RateLimitExceededException(Guid userId)
            : base($"User '{userId}' exceeded the referral creation budget.")
        {
        }

        public override string ErrorCode => "referral.rate_limited";

        public override HttpStatusCode StatusCode => HttpStatusCode.TooManyRequests;
    }
}
