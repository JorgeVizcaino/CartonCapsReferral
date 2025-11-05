using Domain.Dto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReferralService
    {
        Task<bool> AcceptReferral(string ReferralID, CancellationToken cancellationToken);
        Task<Referral> CreateAsync(ReferralRequest request, CancellationToken cancellationToken);
        Task<(bool validated, string referralId)> ValidateSlug(ReferralValidations request, CancellationToken cancellationToken);
    }
}
