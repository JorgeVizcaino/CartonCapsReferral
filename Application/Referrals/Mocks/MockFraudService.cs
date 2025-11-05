using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Referrals.Mocks
{
    public class MockFraudService : IFraudService
    {
        /// <summary>
        /// Stores per-user action timestamps used to enforce a rate limit.
        /// </summary>
        private readonly Dictionary<Guid, List<DateTimeOffset>> _userActions = new();

        /// <summary>
        /// The time window during which user actions are counted toward the rate limit.
        /// </summary>
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The maximum number of allowed create actions per user within the specified time window.
        /// </summary>
        private const int MaxCreatesPerWindow = 2;

        /// <summary>
        /// Determines whether the given user is currently blocked.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// Always returns <see langword="false"/> in this mock implementation,
        /// indicating that no users are blocked.
        /// </returns>
        public Task<bool> IsBlockedAsync(Guid userId)
            => Task.FromResult(false);

        /// <summary>
        /// Checks whether the specified user is still within the allowed number
        /// of create actions for the current time window.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A task that returns <see langword="true"/> if the user has not exceeded
        /// the allowed number of creates within the current window; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method simulates rate-limiting logic by tracking timestamps of recent actions.
        /// Old entries outside the one-minute window are automatically removed on each check.
        /// </remarks>
        public Task<bool> WithinCreateBudgetAsync(Guid userId)
        {
            var now = DateTimeOffset.UtcNow;

            if (!_userActions.TryGetValue(userId, out var timestamps))
            {
                timestamps = new List<DateTimeOffset>();
                _userActions[userId] = timestamps;
            }

            // Remove timestamps older than the current window
            timestamps.RemoveAll(t => now - t > Window);

            if (timestamps.Count >= MaxCreatesPerWindow)
                return Task.FromResult(false);

            timestamps.Add(now);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Validates a vendor-provided signature string.
        /// </summary>
        /// <param name="signature">The vendor signature to validate.</param>
        /// <returns>
        /// A task that returns <see langword="true"/> if the signature is null, empty,
        /// or has a length of at least 10 characters; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This simple check is intended for mock or test use only.
        /// </remarks>
        public Task<bool> ValidateVendorSignatureAsync(string? signature)
            => Task.FromResult(string.IsNullOrEmpty(signature) || signature.Length >= 10);
    }

}
