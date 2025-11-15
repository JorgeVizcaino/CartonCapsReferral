using Application.Interfaces;
using api.Contracts;
using Domain.Dto;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferralsController : ControllerBase
    {
        private readonly IReferralService referralService;
        private readonly IUserServices userServices;

        public ReferralsController(IReferralService referralService, IUserServices userServices)
        {
            this.referralService = referralService;
            this.userServices = userServices;
        }

        private async Task<Guid> RequireUserIdAsync(CancellationToken cancellationToken)
        {
            var user = await userServices.GetUserAsync(cancellationToken);
            return user.Id;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReferralResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReferralResponse>>> GetReferrals([FromQuery] ReferralStatus? status, CancellationToken cancellationToken)
        {
            var userId = await RequireUserIdAsync(cancellationToken);
            var referrals = await referralService.GetReferralsAsync(userId, status, cancellationToken);

            return Ok(referrals.Select(ReferralResponse.FromEntity));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReferralResponse>> GetReferralById(Guid id, CancellationToken cancellationToken)
        {
            var userId = await RequireUserIdAsync(cancellationToken);
            var referral = await referralService.GetReferralByIdAsync(id, userId, cancellationToken);

            return Ok(ReferralResponse.FromEntity(referral));
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(ReferralStatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReferralStatsResponse>> GetStats(CancellationToken cancellationToken)
        {
            var userId = await RequireUserIdAsync(cancellationToken);
            var stats = await referralService.GetStatsAsync(userId, cancellationToken);

            return Ok(ReferralStatsResponse.FromModel(stats));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReferralResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ReferralResponse>> CreateReferral([FromBody] ReferralRequest request, CancellationToken cancellationToken)
        {
            request.UserId = await RequireUserIdAsync(cancellationToken);
            var referral = await referralService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetReferralById), new { id = referral.Id }, ReferralResponse.FromEntity(referral));
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(ReferralValidationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        public async Task<ActionResult<ReferralValidationResponse>> Validate([FromBody] ReferralValidations body, CancellationToken cancellationToken)
        {
            var result = await referralService.ValidateSlugAsync(body, cancellationToken);

            return Ok(new ReferralValidationResponse
            {
                Validated = result.Validated,
                Token = result.Token
            });
        }

        [HttpGet("deeplink/{slug}")]
        [ProducesResponseType(typeof(DeepLinkMetadataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeepLinkMetadataResponse>> GetDeepLinkMetadata(string slug, CancellationToken cancellationToken)
        {
            var referral = await referralService.GetDeepLinkMetadataAsync(slug, cancellationToken);
            return Ok(DeepLinkMetadataResponse.FromEntity(referral));
        }

        [HttpPost("{token}/accept")]
        [ProducesResponseType(typeof(AcceptReferralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        public async Task<ActionResult<AcceptReferralResponse>> Accept(string token, CancellationToken cancellationToken)
        {
            var referral = await referralService.AcceptReferralAsync(token, cancellationToken);

            return Ok(new AcceptReferralResponse
            {
                ReferralId = referral.Id,
                Status = referral.Status.ToString(),
                TokenConsumedUtc = referral.TokenConsumedUtc
            });
        }
    }
}
