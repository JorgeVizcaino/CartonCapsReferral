using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserApp : BaseAuditableEntity
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = default!;
        public string ReferralCode { get; set; } = default!;
    }
}
