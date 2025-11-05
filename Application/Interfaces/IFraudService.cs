using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFraudService
    {
        Task<bool> IsBlockedAsync(Guid userId);
        Task<bool> WithinCreateBudgetAsync(Guid userId); // per-user rate limiter
        Task<bool> ValidateVendorSignatureAsync(string? signature);
    }
}
