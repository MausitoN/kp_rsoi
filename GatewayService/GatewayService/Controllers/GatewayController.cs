using GatewayService.Interfaces;
using GatewayService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GatewayService.Controllers
{
    [Authorize]
    [ApiController]
    //[Route("gate")]
    public class GatewayController : ControllerBase
    {
        private readonly IGatewayService _service;
        private readonly ICircuiteBreaker _circuitBreaker;

        public GatewayController(IGatewayService service, ICircuiteBreaker circuitBreaker)
        {
            _service = service;
            _circuitBreaker = circuitBreaker;
        }

        [AllowAnonymous]
        [HttpGet("cars")]
        public async Task<IActionResult> GetCars()
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetCars()).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpGet("office/{officeUid}/cars")]
        public async Task<IActionResult> GetOfficeCars([FromRoute][Required] Guid officeUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetOfficeCars(officeUid)).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpGet("office/car/{carUid}")]
        public async Task<IActionResult> GetOfficesCar([FromRoute][Required] Guid carUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetOfficesCar(carUid)).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpGet("office/{officeUid}/car/{carUid}")]
        public async Task<IActionResult> GetOfficeCar([FromRoute][Required] Guid officeUid, [FromRoute][Required] Guid carUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetOfficeCar(officeUid, carUid)).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpGet("offices")]
        public async Task<IActionResult> GetOffices()
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetOffices()).ConfigureAwait(false);

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

        [HttpGet("booking/{bookingUid}")]
        public async Task<IActionResult> GetBooking([FromRoute][Required] Guid bookingUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetBooking(bookingUid)).ConfigureAwait(false);

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

        [HttpGet("bookings/{userUid}")]
        public async Task<IActionResult> GetBookings([FromRoute][Required] Guid userUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetBookings(userUid)).ConfigureAwait(false);

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

        [HttpPost("booking")]
        public async Task<IActionResult> CreateBooking([FromBody][Required] CreateBookingRequest request)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.CreateBooking(request)).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetUsers()).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpPost("user")]
        public async Task<IActionResult> AddNewUser([FromBody][Required] AddUserRequest request)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.AddNewUser(request)).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpGet("report/booking-by-models")]
        public async Task<IActionResult> GetReportsBookingByModel()
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetReportsBookingByModel()).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpGet("report/booking-by-offices")]
        public async Task<IActionResult> GetReportsBookingByOffice()
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.GetReportsBookingByOffice()).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpPost("car")]
        public async Task<IActionResult> AddNewCar([FromBody][Required] AddCarRequest request)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.AddNewCar(request)).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpPost("office/{officeUid}/car/{carUid}")]
        public async Task<IActionResult> AddCarToOffice([FromRoute][Required] Guid officeUid, [FromRoute][Required] Guid carUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.AddCarToOffice(officeUid, carUid)).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpDelete("car/{carUid}")]
        public async Task<IActionResult> DeleteCar([FromRoute][Required] Guid carUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.DeleteCar(carUid)).ConfigureAwait(false);

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

        [Authorize(Roles = "admin")]
        [HttpDelete("office/{officeUid}/car/{carUid}")]
        public async Task<IActionResult> DeleteCarFromOffice([FromRoute][Required] Guid officeUid, [FromRoute][Required] Guid carUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.DeleteCarFromOffice(officeUid, carUid)).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpPost("payment/{paymentUid}/pay")]
        public async Task<IActionResult> PayBooking([FromRoute][Required] Guid paymentUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.PayBooking(paymentUid)).ConfigureAwait(false);

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
        public async Task<IActionResult> CancelBooking([FromRoute][Required] Guid bookingUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.CancelBooking(bookingUid)).ConfigureAwait(false);

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

        [AllowAnonymous]
        [HttpPost("booking/{bookingUid}/finish")]
        public async Task<IActionResult> FinishBooking([FromRoute][Required] Guid bookingUid)
        {
            //ServiceResponseGateway response = await _service.GetCars();
            ServiceResponseGateway response = await _circuitBreaker
                .ExecuteActionAsync(() => _service.FinishBooking(bookingUid)).ConfigureAwait(false);

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
    }
}
