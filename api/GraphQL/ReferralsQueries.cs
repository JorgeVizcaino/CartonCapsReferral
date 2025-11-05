using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.GraphQL
{
    [ExtendObjectType("Query")]
    public class ReferralsQueries
    {
        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Referral> GetReferral([Service] IAppDbContext context) => context.Referrals.AsNoTracking();

        [UseProjection]
        public Task<Referral?> GeReferralById([Service] IAppDbContext context, Guid id)
            => context.Referrals.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }
}
