using Application.Interfaces;
using Application.Referrals.Models;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Referrals.Services
{
    public class ReferralsServices : IReferralService
    {
        private readonly IFraudService fraudService;
        private readonly IAppDbContext dbContext;
        private readonly ILinkServices linkServices;

        public ReferralsServices(IFraudService fraudService, IAppDbContext dbContext, ILinkServices linkServices)
        {
            this.fraudService = fraudService;
            this.dbContext = dbContext;
            this.linkServices = linkServices;
        }

        public async Task<Referral> CreateAsync(ReferralRequest request, CancellationToken cancellationToken)
        {
            if (await fraudService.IsBlockedAsync(request.UserId))
            {
                throw new FraudBlockedException(request.UserId);

            }

            if (!await fraudService.WithinCreateBudgetAsync(request.UserId))
            {
                throw new RateLimitExceededException(request.UserId);
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new UserNotFoundException(request.UserId);


            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await dbContext.Referrals.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReferrerUserId == request.UserId &&
                                              r.IdempotencyKey == request.IdempotencyKey, cancellationToken);

                if (existing != null)
                {
                    return existing;

                }
            }

            var referral = Referral.Create(user.Id, user.ReferralCode, request.Campaign, request.IdempotencyKey);
            var linkInfo = await linkServices.CreateAsync(referral);
            referral.Link = linkInfo.Url;
            referral.Token = linkInfo.Token;
            referral.Slug = linkInfo.Slug;
            referral.ExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

            dbContext.Referrals.Add(referral);
            await dbContext.SaveChangesAsync(cancellationToken);
            return referral;
        }

        public async Task<ReferralValidationResult> ValidateSlugAsync(ReferralValidations request, CancellationToken cancellationToken)
        {
            var validated = await fraudService.ValidateVendorSignatureAsync(request.VendorSignature);
            if (!validated)
            {
                throw new ReferralValidationException("Vendor signature validation failed.");
            }

            var now = DateTimeOffset.UtcNow;
            var referral = await dbContext.Referrals
                .FirstOrDefaultAsync(x => x.ReferralCode == request.ReferralCode &&
                                          x.Slug == request.Slug &&
                                          (x.Status == ReferralStatus.Pending || x.Status == ReferralStatus.Reserved), cancellationToken);

            if (referral == null)
            {
                throw new ReferralNotFoundException($"No referral matched slug '{request.Slug}'.");
            }

            if (referral.ExpiresAt.HasValue && referral.ExpiresAt.Value <= now)
            {
                throw new ReferralExpiredException(referral.Id);
            }

            referral.Reserve(now);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new ReferralValidationResult
            {
                Validated = true,
                Token = referral.Token
            };
        }

        public async Task<Referral> AcceptReferralAsync(string token, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ReferralValidationException("Token is required.");
            }

            var referral = await dbContext.Referrals.FirstOrDefaultAsync(x => x.Token == token, cancellationToken)
                ?? throw new ReferralNotFoundException($"Referral token '{token}' not found.");    

            referral.Accept(DateTimeOffset.UtcNow);

            await dbContext.SaveChangesAsync(cancellationToken);
            return referral;
        }

        public async Task<IReadOnlyCollection<Referral>> GetReferralsAsync(Guid userId, ReferralStatus? status, CancellationToken cancellationToken)
        {
            var query = dbContext.Referrals.AsNoTracking().Where(r => r.ReferrerUserId == userId);
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status);
            }

            var records = await query.ToListAsync(cancellationToken); 

            return records
                .OrderByDescending(r => r.CreatedAtUtc)
                .ThenByDescending(r => r.ReservedAtUtc ?? DateTimeOffset.MinValue)
                .ToList();
        }

        public async Task<Referral> GetReferralByIdAsync(Guid referralId, Guid userId, CancellationToken cancellationToken)
        {
            var referral = await dbContext.Referrals.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == referralId && r.ReferrerUserId == userId, cancellationToken);

            if (referral == null)
            {
                throw new ReferralNotFoundException($"Referral '{referralId}' not found for the given user.");
            }

            return referral;
        }

        public async Task<ReferralStats> GetStatsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var counts = await dbContext.Referrals.AsNoTracking()
                .Where(r => r.ReferrerUserId == userId)
                .GroupBy(r => r.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
             

            var dict = counts.ToDictionary(k => k.Key, v => v.Count);
            return ReferralStats.FromCounts(dict);
        }

        public async Task<Referral> GetDeepLinkMetadataAsync(string slug, CancellationToken cancellationToken)
        {
            var referral = await dbContext.Referrals.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Slug == slug, cancellationToken);



            if (referral == null)
            {
                throw new ReferralNotFoundException($"Referral with slug '{slug}' was not found.");
            }


            return referral;
        }
    }
}
