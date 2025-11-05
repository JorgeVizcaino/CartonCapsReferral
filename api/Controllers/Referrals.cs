using Application.Interfaces;
using Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Referrals : ControllerBase
    {
        private async Task<Guid> RequireUserIdAsync(IUserServices userService, CancellationToken cancellationToken)
        {
            var user = await userService.GetUsers(cancellationToken);
            if (user != null)
            {
                return user.Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReferrals([FromServices] IReferralService ReferralService,
            [FromServices] IUserServices userService,
            [FromBody] ReferralRequest request,
            CancellationToken cancellationToken)
        {
            request.UserId = await RequireUserIdAsync(userService,cancellationToken);

            var registration = await ReferralService.CreateAsync(request, cancellationToken);
            return Created($"/api/referrals/{registration.Id}", registration);
        }



        // POST v1/referrals/resolve the URL sent
        [HttpPost("ReferralValidation")]
        public async Task<IActionResult> ReferralValidation([FromBody] ReferralValidations body,
            [FromServices] IReferralService ReferralService,
             CancellationToken cancellationToken)
        {
            var referralValidated = await ReferralService.ValidateSlug(body, cancellationToken);
            if (referralValidated.validated)
            {
                return Ok(new { status = "Validated", tokenID = referralValidated.referralId });
            }
            else
            {
                return BadRequest("Invalid ReferralCode");
            }
        }     


        // POST v1/referrals/{id}/accept
        [HttpPost("{tokenID}/accept")]
        public async Task<IActionResult> Accept([FromRoute] string tokenID,
             [FromServices] IReferralService ReferralService,
             CancellationToken cancellationToken)
        {
            var response = await ReferralService.AcceptReferral(tokenID, cancellationToken);

            if(!response)
                return BadRequest("Invalid Token");

            return Ok(new { status = "accepted" });
        }
    }
}
