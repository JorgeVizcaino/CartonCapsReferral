using System;
using System.Net;

namespace Domain.Exceptions
{
    public sealed class FraudBlockedException : DomainException
    {
        public FraudBlockedException(Guid userId)
            : base($"User '{userId}' is temporarily blocked from creating referrals.")
        {
        }

        public override string ErrorCode => "referral.fraud_blocked";

        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    }
}
