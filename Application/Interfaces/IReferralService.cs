using Application.Referrals.Models;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IReferralService
    {
        Task<Referral> CreateAsync(ReferralRequest request, CancellationToken cancellationToken);
        Task<ReferralValidationResult> ValidateSlugAsync(ReferralValidations request, CancellationToken cancellationToken);
        Task<Referral> AcceptReferralAsync(string token, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Referral>> GetReferralsAsync(Guid userId, ReferralStatus? status, CancellationToken cancellationToken);
        Task<Referral> GetReferralByIdAsync(Guid referralId, Guid userId, CancellationToken cancellationToken);
        Task<ReferralStats> GetStatsAsync(Guid userId, CancellationToken cancellationToken);
        Task<Referral> GetDeepLinkMetadataAsync(string slug, CancellationToken cancellationToken);
    }
}
