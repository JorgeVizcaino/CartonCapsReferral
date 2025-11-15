using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.User.Service
{
    public class UserServices : IUserServices
    {
        private readonly IAppDbContext context;

        public UserServices(IAppDbContext context)
        {
            this.context = context;
        }


        public async Task<UserApp> GetUserAsync(CancellationToken cancellationToken)
        {

            var user = await context.Users.FirstOrDefaultAsync(x => x.DisplayName == "Sam", cancellationToken);
            if (user == null)
            {
                throw new UserNotFoundException("Sam");
            }
            return user;

        }

    }
}
