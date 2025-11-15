using System;
using System.Net;

namespace Domain.Exceptions
{
    public sealed class UserNotFoundException : DomainException
    {
        public UserNotFoundException(Guid userId)
            : base($"User '{userId}' was not found.")
        {
        }

        public UserNotFoundException(string displayName)
            : base($"User '{displayName}' was not found.")
        {
        }

        public override string ErrorCode => "user.not_found";

        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    }
}
