using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<UserApp> Users => Set<UserApp>();
        public DbSet<Referral> Referrals => Set<Referral>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Referral>(entity =>
            {
                entity.Property(r => r.ReferralCode).IsRequired();
                entity.Property(r => r.Token).IsRequired();
                entity.Property(r => r.Slug).IsRequired();
                entity.Property(r => r.Status).HasConversion<int>();

                entity.HasIndex(r => r.Token).IsUnique();
                entity.HasIndex(r => r.Slug).IsUnique();
                entity.HasIndex(r => new { r.ReferrerUserId, r.IdempotencyKey }).IsUnique();
            });
        }
    }
}
