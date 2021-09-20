using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeService.Interfaces;
using OfficeService.Models;
using ShareService.JwtGenerator;

namespace Office.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class OfficeController : ControllerBase
    {
        private readonly IOfficeService _service;
        private readonly ITokenGeneratorService _tokenGenerator;
        public OfficeController(IOfficeService service,
                                ITokenGeneratorService tokenGenerator)
        {
            _service = service;
            _tokenGenerator = tokenGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOfficesInfo()
        {
            ServiceResponseOffice response = await _service.GetAllOffices();

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet("{officeUid}")]
        public async Task<IActionResult> GetOfficeInfo([FromRoute][Required] Guid officeUid)
        {
            ServiceResponseOffice response = await _service.GetOffice(officeUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet("{officeUid}/car")]
        public async Task<IActionResult> GetAllCarsOfficeInfo([FromRoute][Required] Guid officeUid)
        {
            ServiceResponseOffice response = await _service.GetAllCarsOffice(officeUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet("{officeUid}/car/{carUid}")]
        public async Task<IActionResult> GetCarOfficeInfo([FromRoute][Required] Guid officeUid, [FromRoute][Required] Guid carUid)
        {
            ServiceResponseOffice response = await _service.GetCarOffice(officeUid, carUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet("car/{carUid}")]
        public async Task<IActionResult> GetCarAllOfficeInfo([FromRoute][Required] Guid carUid)
        {
            ServiceResponseOffice response = await _service.GetCarAllOffice(carUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost("{officeUid}/car/{carUid}")]
        public async Task<IActionResult> AddCarToOffice([FromRoute][Required] Guid officeUid, [FromRoute][Required] Guid carUid)
        {
            ServiceResponseOffice response = await _service.AddCarToOffice(officeUid, carUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnCarToOffice([FromBody][Required] ReturnCarRequest request)
        {
            ServiceResponseOffice response = await _service.ReturnCarToOffice(request);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpDelete("{officeUid}/car/{carUid}")]
        public async Task<IActionResult> DeleteCarFromOffice([FromRoute][Required] Guid officeUid, [FromRoute][Required] Guid carUid)
        {
            ServiceResponseOffice response = await _service.DeleteCarFromOffice(officeUid, carUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
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
