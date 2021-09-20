using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SessionService.Interfaces;
using SessionService.Models;
using ShareService.JwtGenerator;

namespace SessionService.Controllers
{
    [Authorize]
    [ApiController]
    //[Route("session")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _service;
        private readonly ITokenGeneratorService _tokenGenerator;
        public SessionController(ISessionService service,
                                 ITokenGeneratorService tokenGenerator)
        {
            _service = service;
            _tokenGenerator = tokenGenerator;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _service.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody][Required] NewUser request)
        {

            var newUser = await _service.CreateUserAsync(request);
            var controllerName = ControllerContext.ActionDescriptor.ControllerName;
            return Created($"{Url.Action(nameof(GetUsers), controllerName)}/{newUser.UserUid}", newUser);
        }

        [Authorize(AuthenticationSchemes = "Basic")]
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            try
            {
                var token = _tokenGenerator.GetToken();
                return Ok(token);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse() { Message = "Internal error" });
            }
        }
    }
}
