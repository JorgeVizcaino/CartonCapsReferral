using System;
using System.Net;

namespace Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }

        public virtual string ErrorCode => "domain_error";

        public virtual HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    }
}
