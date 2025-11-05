using Application.Interfaces;
using Domain.Entities;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Referrals.Services
{
    public class LinkServices : ILinkServices
    {
        private const string BaseUrl = "https://ccappReferrals.link/";
        /// <summary>
        /// Generates a new link and token for a referral.
        /// </summary>
        /// <param name="referral">The referral entity to generate data for.</param>
        /// <returns>A tuple containing the URL and single-use token.</returns>
        public Task<(string Url, string Token, string slug)> CreateAsync(Referral referral)
        {
            // Generate a unique, hard-to-guess token (base64 + referral ID)
            string token = GenerateSecureToken(referral.Id);


            // Encode a short identifier for the URL slug
            string slug = GenerateSlug(referral);

            string url = $"{BaseUrl}{slug}";

            return Task.FromResult((url, token, slug));
        }


        /// <summary>
        /// Generates a secure token by combining the referral ID with random entropy.
        /// </summary>
        private static string GenerateSecureToken(Guid referralId)
        {
            byte[] entropy = RandomNumberGenerator.GetBytes(16);
            string randomPart = Convert.ToBase64String(entropy).Replace("/", "_").Replace("+", "-").TrimEnd('=');
            return $"{referralId:N}-{randomPart}";
        }


        /// <summary>
        /// Generates a short slug for a deep link, suitable for embedding in URLs.
        /// </summary>
        private static string GenerateSlug(Referral referral)
        {
            string input = $"{referral.ReferralCode}-{referral.Id}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Substring(0, 10);
        }
    }

}
