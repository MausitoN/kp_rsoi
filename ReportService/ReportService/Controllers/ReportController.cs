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
using ReportService.Interfaces;
using ReportService.Models;
using ShareService.JwtGenerator;

namespace ReportService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("report")]
    public class CarController : ControllerBase
    {
        private readonly IReportService _service;
        private readonly ITokenGeneratorService _tokenGenerator;
        public CarController(IReportService service,
                             ITokenGeneratorService tokenGenerator)
        {
            _service = service;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> AddReport([FromBody][Required] ReportRequest request)
        {

            ServiceResponseReport response = await _service.AddReport(request);

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

        [HttpGet("booking-by-models")]
        public async Task<IActionResult> GetModelInfo()
        {
            ServiceResponseReport response = await _service.GetModel();

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

        [HttpGet("booking-by-offices")]
        public async Task<IActionResult> GetLocationInfo()
        {
            ServiceResponseReport response = await _service.GetLocation();

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

        [HttpPost("check")]
        public async Task<IActionResult> GetNewReport()
        {
            ServiceResponseReport response = await _service.CheckQueue();

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