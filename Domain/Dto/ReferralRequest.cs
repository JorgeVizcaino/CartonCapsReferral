using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ReferralRequest
    {        
        public string? Campaign { get; set; }
        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}
