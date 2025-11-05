using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public interface IAppDbContext
    {
        DbSet<Referral> Referrals { get; }
        DbSet<UserApp> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}