using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ReferralValidations
    {
        public required string ReferralCode { get; set; }

        public required string Slug { get; set; }
    }
}
