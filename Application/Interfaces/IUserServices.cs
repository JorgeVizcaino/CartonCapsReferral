using Domain.Dto;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<UserApp> GetUsers(CancellationToken cancellationToken);
    }
}