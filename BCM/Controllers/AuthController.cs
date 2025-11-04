using BCM.Managment.Auth.DTOs;
using BCM.Managment.Auth.Manager;
using BCM.Managment.Base.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BCM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthManager _authManager) : ControllerBase
    {
        /// <summary>
        /// Authenticates a user and returns a login token.
        /// </summary>
        /// <param name="request">The login request containing user credentials.</param>
        /// <returns>An action result with the authentication response.</returns>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(DefaultResponse<object>.FailureResponse(
                    message_en: "Invalid model state",
                    message_ar: "حالة النموذج غير صالحة"
                    ));
            }

            var result = await _authManager.Login(request);

            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

    }
}
