using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BookingService.Interfaces;
using BookingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Logging;
using ShareService.JwtGenerator;

namespace CarBookingSystem.Controllers
{
    [Authorize]
    [ApiController]
    //[Route("booking")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly ITokenGeneratorService _tokenGenerator;
        public BookingController(IBookingService service,
                             ITokenGeneratorService tokenGenerator)
        {
            _service = service;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("booking")]
        public async Task<IActionResult> CreateBooking([FromBody][Required] CreateBookingRequest request)
        {
            ServiceResponseBooking response = await _service.CreateBooking(request);

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

        [HttpDelete("booking/{bookingUid}")]
        public async Task<IActionResult> DeleteBooking([FromRoute][Required] Guid bookingUid)
        {
            ServiceResponseBooking response = await _service.DeleteBooking(bookingUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = (response.StatusCode == 204) ? String.Empty : JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPatch("booking/{bookingUid}/finish")]
        public async Task<IActionResult> FinishBooking([FromRoute][Required] Guid bookingUid)
        {
            ServiceResponseBooking response = await _service.FinishBooking(bookingUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = (response.StatusCode == 204) ? String.Empty : JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet("booking/{bookingUid}")]
        public async Task<IActionResult> GetBooking([FromRoute][Required] Guid bookingUid)
        {
            ServiceResponseBooking response = await _service.GetBookingInfo(bookingUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = (response.StatusCode == 204) ? String.Empty : JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet("bookings/{userUid}")]
        public async Task<IActionResult> GetBookings([FromRoute][Required] Guid userUid)
        {
            ServiceResponseBooking response = await _service.GetBookingsInfo(userUid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var result = new ContentResult
            {
                StatusCode = response.StatusCode,
                Content = (response.StatusCode == 204) ? String.Empty : JsonSerializer.Serialize(response.Result, options),
                ContentType = "application/json"
            };

            return result;
        }

        [Authorize(AuthenticationSchemes = "Basic")]
        [HttpGet("booking/token")]
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
