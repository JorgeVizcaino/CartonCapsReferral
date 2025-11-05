using Application.Interfaces;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using System.Collections.Generic;


namespace Application.Referrals.Services
{
    public class ReferralsServices : IReferralService
    {
        private readonly IFraudService fraud;
        private readonly IAppDbContext dbcontext;
        private readonly ILinkServices link;
        private readonly IFraudService fraudService;

        public ReferralsServices(IFraudService fraud,
            IAppDbContext dbcontext,
            ILinkServices link,
            IFraudService fraudService)
        {
            this.fraud = fraud;
            this.dbcontext = dbcontext;
            this.link = link;
            this.fraudService = fraudService;
        }


        public async Task<Referral> CreateAsync(ReferralRequest request, CancellationToken cancellationToken)
        {
            if (await fraud.IsBlockedAsync(request.UserId))
                throw new Exception("user is temporarily blocked");


            if (!await fraud.WithinCreateBudgetAsync(request.UserId))
                throw new Exception("Too Many Requests");

            var user = await dbcontext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            var referral = Referral.Create(user.Id, user.ReferralCode, request.Campaign);
            var linkInfo = await link.CreateAsync(referral);
            referral.Link = linkInfo.Url;
            referral.Token = linkInfo.Token;
            referral.Slug = linkInfo.slug;
            referral.ExpiresAt = DateTimeOffset.UtcNow.AddDays(30);


            dbcontext.Referrals.Add(referral);
            await dbcontext.SaveChangesAsync(cancellationToken);
            return referral;

        }


        public async Task<(bool validated, string referralId)> ValidateSlug(ReferralValidations request, CancellationToken cancellationToken)
        {

            var vallidated = await fraudService.ValidateVendorSignatureAsync(request.Slug);
            if (vallidated == false)
                return (false, string.Empty);

            var referral = await dbcontext.Referrals.FirstOrDefaultAsync(x => x.ReferralCode == request.ReferralCode
                                && x.Slug == request.Slug
                                && x.Status == ReferralStatus.Pending
                                && x.ExpiresAt > DateTimeOffset.UtcNow);

            if (referral == null)
            {
                return (false, string.Empty);
                
            }          

            return (true, referral.Token);

        }

        public async Task<bool> AcceptReferral(string ReferralID, CancellationToken cancellationToken)
        {
         

            var referral = await dbcontext.Referrals.FirstOrDefaultAsync(x => x.Token == ReferralID);

            if (referral == null)
            {
                return false;

            }       

            referral.Status = ReferralStatus.Accepted;
            referral.TokenConsumedUtc = DateTimeOffset.UtcNow;
            await dbcontext.SaveChangesAsync(cancellationToken);

            return true;

        }


    }
}
