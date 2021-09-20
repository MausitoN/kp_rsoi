using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SessionService.Interfaces;
using SessionService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace SessionService.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [Authorize(AuthenticationSchemes = "UserBasic")]
        [HttpPost("auth")]
        public IActionResult GetToken()
        {
            var userUid = new Guid(User.Claims.First(c => c.Type == ClaimTypes.UserData).Value);
            var tokenResult = _service.GetAccessToken(User.Claims);
            return Ok(tokenResult);

        }

        [AllowAnonymous]
        [HttpPost("verify")]
        [ProducesResponseType(typeof(TokenModel), StatusCodes.Status200OK)]
        public IActionResult Verify()
        {
            try
            {
                if (!Request.Headers.ContainsKey("Authorization"))
                    return Unauthorized(new ErrorResponse { Message = "Missing Authorization Header" });

                var verifyToken = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                if (string.IsNullOrWhiteSpace(verifyToken?.Parameter) || string.IsNullOrWhiteSpace(verifyToken?.Scheme))
                    return Unauthorized(new ErrorResponse { Message = "Missing Bearer token in Authorization Header" });

                if (_service.ValidateToken(verifyToken.Parameter))
                {
                    return Ok();
                }
                else
                    return Unauthorized();
            }
            catch (SecurityTokenExpiredException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse() { Message = "Internal error:" });
            }
        }
    }
}
