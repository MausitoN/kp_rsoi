using GatewayService.Interfaces;
using GatewayService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShareService.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    public class GatewayServices : IGatewayService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ServiceCredentials _serviceCredentials;

        public GatewayServices(IHttpClientFactory clientFactory,
                               IConfiguration configuration,
                               ITokenService tokenService,
                               IOptions<ServiceCredentials> secretOptions)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _serviceCredentials = secretOptions?.Value;
        }

        // Получение списка автомобилей
        public async Task<ServiceResponseGateway> GetCars()
        {
            try
            {
                var token = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeCar())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["CarServiceUri"]}/car");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetCarServiceToken());
                HttpResponseMessage response = await client.GetAsync($"{_configuration["CarServiceUri"]}/car");
                //HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "CarService is not available" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Car not found" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeCar())
                            return await GetCars();
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<Automobile> car = await JsonSerializer.DeserializeAsync
                    <List<Automobile>>(responseStream, options);

                return new ServiceResponseGateway(car);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (CarService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение списка автомобилей в офисе
        public async Task<ServiceResponseGateway> GetOfficeCars(Guid officeUid)
        {
            try
            {
                var token = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeCar())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["OfficeServiceUri"]}/office/{officeUid}/car");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await GetOfficeCars(officeUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Available car not found" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (OfficeService, CarService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<AllCarInOffice> officeCars = await JsonSerializer.DeserializeAsync
                    <List<AllCarInOffice>>(responseStream, options);

                return new ServiceResponseGateway(officeCars);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (OfficeService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение списка офисов, в которых есть данный автомобиль
        public async Task<ServiceResponseGateway> GetOfficesCar(Guid carUid)
        {
            try
            {
                var token = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["OfficeServiceUri"]}/office/car/{carUid}");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await GetOfficesCar(carUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Office not found" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (OfficeService, CarService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<AllCarInOffice> carOffices = await JsonSerializer.DeserializeAsync
                    <List<AllCarInOffice>>(responseStream, options);

                return new ServiceResponseGateway(carOffices);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (OfficeService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение информации о доступности автомобиля в офисе
        public async Task<ServiceResponseGateway> GetOfficeCar(Guid officeUid, Guid carUid)
        {
            try
            {
                var token = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["OfficeServiceUri"]}/office/{officeUid}/car/{carUid}");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await GetOfficeCar(officeUid, carUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (OfficeService, CarService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                AllCarInOffice carOffice = await JsonSerializer.DeserializeAsync
                    <AllCarInOffice>(responseStream, options);

                return new ServiceResponseGateway(carOffice);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (OfficeService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение списка офисов 
        public async Task<ServiceResponseGateway> GetOffices()
        {
            try
            {
                var token = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["OfficeServiceUri"]}/office");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "OfficeService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await GetOffices();
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Office not found" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<Office> offices = await JsonSerializer.DeserializeAsync
                    <List<Office>>(responseStream, options);

                return new ServiceResponseGateway(offices);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (OfficeService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение информации о брони
        public async Task<ServiceResponseGateway> GetBooking(Guid bookingUid)
        {
            try
            {
                var token = _tokenService.GetBookingServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeBooking())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["BookingServiceUri"]}/booking/{bookingUid}");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetBookingServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeBooking())
                            return await GetBooking(bookingUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Booking not found" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (BookingService, CarService, OfficeService, PaymentService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                AllBookingInfo booking = await JsonSerializer.DeserializeAsync
                    <AllBookingInfo>(responseStream, options);

                return new ServiceResponseGateway(booking);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (BookingService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение списка брони (вообще должен передаваться userUid)
        public async Task<ServiceResponseGateway> GetBookings(Guid userUid)
        {
            try
            {
                var token = _tokenService.GetBookingServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeBooking())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["BookingServiceUri"]}/bookings/{userUid}");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetBookingServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeBooking())
                            return await GetBookings(userUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Booking not found" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (BookingService, CarService, OfficeService, PaymentService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<Booking> booking = await JsonSerializer.DeserializeAsync
                    <List<Booking>>(responseStream, options);

                return new ServiceResponseGateway(booking);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (BookingService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Создание брони
        public async Task<ServiceResponseGateway> CreateBooking(CreateBookingRequest request)
        {
            try
            {
                var token = _tokenService.GetBookingServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeBooking())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                        $"{_configuration["BookingServiceUri"]}/booking");

                httpRequest.Content = new StringContent(JsonSerializer.Serialize(new CreateBookingRequest
                {
                    CarUid = request.CarUid,
                    RegistrationNumber = request.RegistrationNumber,
                    TakenFromOffice = request.TakenFromOffice,
                    ReturnToOffice = request.ReturnToOffice,
                    BookingPeriod = request.BookingPeriod,
                    UserUid = request.UserUid
                }));

                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = _clientFactory.CreateClient(); 
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetBookingServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeBooking())
                            return await CreateBooking(request);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (BookingService, CarService, OfficeService, PaymentService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(201);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (BookingService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение списка пользователей
        public async Task<ServiceResponseGateway> GetUsers()
        {
            try
            {
                var token = _tokenService.GetSessionServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeSession())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["SessionServiceUri"]}/users");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetSessionServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "SessionService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeSession())
                            return await GetUsers();
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "User not found" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<User> users = await JsonSerializer.DeserializeAsync
                    <List<User>>(responseStream, options);

                return new ServiceResponseGateway(users);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (SessionService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Добавление нового пользователя
        public async Task<ServiceResponseGateway> AddNewUser(AddUserRequest request)
        {
            try
            {
                var token = _tokenService.GetSessionServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeSession())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                        $"{_configuration["SessionServiceUri"]}/user");

                httpRequest.Content = new StringContent(JsonSerializer.Serialize(new AddUserRequest
                {
                    Login = request.Login,
                    Password = request.Password
                }));

                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetSessionServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "SessionService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeSession())
                            return await AddNewUser(request);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(201);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (SessionService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение статистики бронирования по моделям
        public async Task<ServiceResponseGateway> GetReportsBookingByModel()
        {
            try
            {
                var token = _tokenService.GetReportServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeReport())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["ReportServiceUri"]}/report/booking-by-models");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetReportServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "ReportService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeReport())
                            return await GetReportsBookingByModel();
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Report not found" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<ModelReport> modelReport = await JsonSerializer.DeserializeAsync
                    <List<ModelReport>>(responseStream, options);

                return new ServiceResponseGateway(modelReport);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (ReportService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Получение статистики по офисам
        public async Task<ServiceResponseGateway> GetReportsBookingByOffice()
        {
            try
            {
                var token = _tokenService.GetReportServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeReport())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["ReportServiceUri"]}/report/booking-by-offices");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetReportServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "ReportService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeReport())
                            return await GetReportsBookingByOffice();
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Report not found" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                List<LocationReport> officeReport = await JsonSerializer.DeserializeAsync
                    <List<LocationReport>>(responseStream, options);

                return new ServiceResponseGateway(officeReport);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (ReportService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Добавление нового автомобиля
        public async Task<ServiceResponseGateway> AddNewCar(AddCarRequest request)
        {
            try
            {
                var token = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeCar())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                        $"{_configuration["CarServiceUri"]}/car");

                httpRequest.Content = new StringContent(JsonSerializer.Serialize(new AddCarRequest
                {
                    Brand = request.Brand,
                    Model = request.Model,
                    Power = request.Power,
                    CarType = request.CarType
                }));

                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetCarServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "CarService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeCar())
                            return await AddNewCar(request);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(201);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (CarService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Добавление автомобиля в офис
        public async Task<ServiceResponseGateway> AddCarToOffice(Guid officeUid, Guid carUid)
        {
            try
            {
                var token = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                        $"{_configuration["OfficeServiceUri"]}/office/{officeUid}/car/{carUid}");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "OfficeService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await AddCarToOffice(officeUid, carUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(204);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (OfficeService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Удаление автомобиля
        public async Task<ServiceResponseGateway> DeleteCar(Guid carUid)
        {
            try
            {
                var token = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeCar())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Delete,
                        $"{_configuration["CarServiceUri"]}/car/{carUid}");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetCarServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "CarService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeCar())
                            return await DeleteCar(carUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 404)
                    {
                        return new ServiceResponseGateway(404, new ErrorResponse { Message = "Car not found" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(204);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (CarService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Удаление автомобиля из офиса
        public async Task<ServiceResponseGateway> DeleteCarFromOffice(Guid officeUid, Guid carUid)
        {
            try
            {
                var token = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Delete,
                        $"{_configuration["OfficeServiceUri"]}/office/{officeUid}/car/{carUid}");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "OfficeService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await DeleteCarFromOffice(officeUid, carUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(204);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (OfficeService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Оплата брони
        public async Task<ServiceResponseGateway> PayBooking(Guid paymentUid)
        {
            try
            {
                var token = _tokenService.GetPaymentServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizePayment())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Patch,
                        $"{_configuration["PaymentServiceUri"]}/payment/{paymentUid}");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetPaymentServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "PaymentService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizePayment())
                            return await PayBooking(paymentUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(204);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (PaymentService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Отмена брони
        public async Task<ServiceResponseGateway> CancelBooking(Guid bookingUid)
        {
            try
            {
                var token = _tokenService.GetBookingServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeBooking())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Delete,
                        $"{_configuration["BookingServiceUri"]}/booking/{bookingUid}");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetBookingServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeBooking())
                            return await CancelBooking(bookingUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "Internal Server Error (BookingService, PaymentService, OfficeService)" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(204);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (BookingService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        // Окончание брони
        public async Task<ServiceResponseGateway> FinishBooking(Guid bookingUid)
        {
            try
            {
                var token = _tokenService.GetBookingServiceToken();
                if (string.IsNullOrEmpty(token))
                    if (!await AuthorizeBooking())
                        return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                var httpRequest = new HttpRequestMessage(HttpMethod.Patch,
                        $"{_configuration["BookingServiceUri"]}/booking/{bookingUid}/finish");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                    _tokenService.GetBookingServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 503)
                    {
                        return new ServiceResponseGateway(503, new ErrorResponse { Message = "BookingService is not available" });
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeBooking())
                            return await FinishBooking(bookingUid);
                        else
                            return new ServiceResponseGateway(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    return new ServiceResponseGateway((int)response.StatusCode, new ErrorResponse { Message = response.ReasonPhrase });
                }

                return new ServiceResponseGateway(204);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseGateway(500, new ErrorResponse { Message = "Internal Server Error (BookingService is not available)" });
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseGateway(500, e.Message);
            }
        }

        public async Task<bool> AuthorizeCar()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceCredentials.ClientId}:{_serviceCredentials.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["CarServiceUri"]}/car/token");
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var token = await response.Content.ReadAsStringAsync(); // ReadAsAsync<string>
                _tokenService.SetCarServiceToken(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthorizeOffice()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceCredentials.ClientId}:{_serviceCredentials.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["OfficeServiceUri"]}/office/token");
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var token = await response.Content.ReadAsStringAsync(); // ReadAsAsync<string>
                _tokenService.SetOfficeServiceToken(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthorizePayment()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceCredentials.ClientId}:{_serviceCredentials.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["PaymentServiceUri"]}/payment/token");
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var token = await response.Content.ReadAsStringAsync(); // ReadAsAsync<string>
                _tokenService.SetPaymentServiceToken(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthorizeBooking()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceCredentials.ClientId}:{_serviceCredentials.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["BookingServiceUri"]}/booking/token");
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var token = await response.Content.ReadAsStringAsync(); // ReadAsAsync<string>
                _tokenService.SetBookingServiceToken(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthorizeSession()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceCredentials.ClientId}:{_serviceCredentials.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["SessionServiceUri"]}/token");
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var token = await response.Content.ReadAsStringAsync(); // ReadAsAsync<string>
                _tokenService.SetSessionServiceToken(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthorizeReport()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceCredentials.ClientId}:{_serviceCredentials.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);

                var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["ReportServiceUri"]}/report/token");
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                var token = await response.Content.ReadAsStringAsync(); // ReadAsAsync<string>
                _tokenService.SetReportServiceToken(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
