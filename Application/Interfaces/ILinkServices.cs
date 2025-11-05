using Domain.Entities;

namespace Application.Interfaces
{
    public interface ILinkServices
    {
        Task<(string Url, string Token, string slug)> CreateAsync(Referral referral);
    }
}