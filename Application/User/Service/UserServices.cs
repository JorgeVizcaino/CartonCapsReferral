using Application.Interfaces;
using Domain.Dto;
using Domain.Entities;
using Infrastructure.Data;

namespace Application.User.Service
{
    public class UserServices : IUserServices
    {
        private readonly IAppDbContext context;

        public UserServices(IAppDbContext context)
        {
            this.context = context;
        }


        public async Task<UserApp> GetUsers(CancellationToken cancellationToken)
        {

            var user = await context.Users.FirstOrDefaultAsync(x => x.DisplayName == "Sam");
            return user;

        }

    }
}
