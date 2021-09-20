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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PaymentService.Interfaces;
using PaymentService.Models;
using ShareService.JwtGenerator;

namespace Payment.Controllers
{
    [Authorize]
    [ApiController]
    [Route("payment")]
    public class CarController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly ITokenGeneratorService _tokenGenerator;
        public CarController(IPaymentService service,
                             ITokenGeneratorService tokenGenerator)
        {
            _service = service;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment()
        {

            ServiceResponsePayment response = await _service.CreatePayment();

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

        [HttpGet("{paymentUid}")]
        public async Task<IActionResult> GetPaymentInfo([FromRoute][Required] Guid paymentUid)
        {

            ServiceResponsePayment response = await _service.GetPaymentInfo(paymentUid);

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

        [HttpDelete("{paymentUid}")]
        public async Task<IActionResult> CancelPayment([FromRoute][Required] Guid paymentUid)
        {

            ServiceResponsePayment response = await _service.CancelPayment(paymentUid);

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

        [HttpPatch("{paymentUid}")]
        public async Task<IActionResult> PayBooking([FromRoute][Required] Guid paymentUid)
        {

            ServiceResponsePayment response = await _service.PayBooking(paymentUid);

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
