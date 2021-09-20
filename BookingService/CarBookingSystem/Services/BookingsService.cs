using BookingService.Interfaces;
using BookingService.Models;
using CarBookingSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
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

namespace BookingService.Services
{
    public class BookingsService : IBookingService
    {
        private readonly BookingContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly IQueueRabbitMQ _queueRabbitMQ;
        private readonly ITokenService _tokenService;
        private readonly ServiceCredentials _serviceCredentials;

        public BookingsService(BookingContext context,
                               IHttpClientFactory clientFactory,
                               IConfiguration configuration,
                               IQueueRabbitMQ queueRabbitMQ,
                               ITokenService tokenService,
                               IOptions<ServiceCredentials> secretOptions)
        {
            _context = context;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _queueRabbitMQ = queueRabbitMQ;
            _tokenService = tokenService;
            _serviceCredentials = secretOptions.Value;
        }

        // Создание брони
        public async Task<ServiceResponseBooking> CreateBooking(CreateBookingRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new ServiceResponseBooking(400, new ErrorResponse { Message = "Bad request format" });
                }

                Guid bookingUid = Guid.NewGuid();

                // Запрос на создание оплаты
                var tokenPayment = _tokenService.GetPaymentServiceToken();
                if (string.IsNullOrEmpty(tokenPayment))
                    if (!await AuthorizePayment())
                        return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    $"{_configuration["PaymentServiceUri"]}/payment");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetPaymentServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizePayment())
                            return await CreateBooking(request);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 409 || (int)response.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, payment not found" });
                    }
                    if ((int)response.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (PaymentService)" });
                    }
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                using var responseStream = await response.Content.ReadAsStreamAsync();
                PaymentResponse payment = await JsonSerializer.DeserializeAsync
                    <PaymentResponse>(responseStream, options);

                var booking = new Booking();
                booking.Id = bookingUid;
                booking.CarUid = request.CarUid;
                booking.UserUid = request.UserUid;
                booking.RegistrationNumber = request.RegistrationNumber;
                booking.PaymentUid = payment.Id;
                booking.BookingPeriod = request.BookingPeriod; //DateTime.UtcNow
                booking.Status = BookingStatus.NEW;
                booking.TakeFromOfficeUid = request.TakenFromOffice;
                booking.ReturnToOfficeUid = request.ReturnToOffice;

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                // Запрос на получение информации об автомобиле для статистики
                var tokenCar = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(tokenCar))
                    if (!await AuthorizeCar())
                        return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequestCar = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["CarServiceUri"]}/car/{booking.CarUid}");

                HttpClient clientCar = _clientFactory.CreateClient();
                clientCar.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetCarServiceToken());
                HttpResponseMessage responseCar = await clientCar.SendAsync(httpRequestCar);

                if (!responseCar.IsSuccessStatusCode)
                {
                    if (responseCar.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeCar())
                            return await CreateBooking(request);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseCar.StatusCode == 409 || (int)responseCar.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, car not found" });
                    }
                    if ((int)responseCar.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (CarService)" });
                    }
                    var httpRequestPayment = new HttpRequestMessage(HttpMethod.Delete,
                    $"{_configuration["PaymentServiceUri"]}/payment/{booking.PaymentUid}");

                    HttpClient clientPayment = _clientFactory.CreateClient();
                    clientPayment.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetPaymentServiceToken());
                    HttpResponseMessage responsePayment = await clientPayment.SendAsync(httpRequestPayment);

                    if (!responsePayment.IsSuccessStatusCode)
                    {
                        return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                    }
                    
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }

                using var responseStreamCar = await responseCar.Content.ReadAsStreamAsync();
                AutomobileResponse car = await JsonSerializer.DeserializeAsync
                    <AutomobileResponse>(responseStreamCar, options);

                // Запрос на получение информации об офисах для статистики
                var tokenOffice = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(tokenOffice))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequestOffice = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["OfficeServiceUri"]}/office/{booking.TakeFromOfficeUid}");

                HttpClient clientOffice = _clientFactory.CreateClient();
                clientOffice.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage responseOffice = await clientOffice.SendAsync(httpRequestOffice);

                if (!responseOffice.IsSuccessStatusCode)
                {
                    if (responseOffice.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await CreateBooking(request);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseOffice.StatusCode == 409 || (int)responseOffice.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, office not found" });
                    }
                    if ((int)responseOffice.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (OfficeService)" });
                    }
                    var httpRequestPayment = new HttpRequestMessage(HttpMethod.Delete,
                    $"{_configuration["PaymentServiceUri"]}/payment/{booking.PaymentUid}");

                    HttpClient clientP = _clientFactory.CreateClient();
                    clientP.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetPaymentServiceToken());
                    HttpResponseMessage responseP = await clientP.SendAsync(httpRequestPayment);

                    if (!responseP.IsSuccessStatusCode)
                    {
                        return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                    }
                    
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }

                using var responseStreamOffice = await responseOffice.Content.ReadAsStreamAsync();
                OfficeResponse office = await JsonSerializer.DeserializeAsync
                    <OfficeResponse>(responseStreamOffice, options);

                // Запрос на удаление  автомобиля из офиса
                var httpRequestOfficeDel = new HttpRequestMessage(HttpMethod.Delete,
                    $"{_configuration["OfficeServiceUri"]}/office/{booking.TakeFromOfficeUid}/car/{booking.CarUid}");

                HttpClient clientOfficeDel = _clientFactory.CreateClient();
                clientOfficeDel.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage responseOfficeDel = await clientOfficeDel.SendAsync(httpRequestOfficeDel);

                if (!responseOfficeDel.IsSuccessStatusCode)
                {
                    if (responseOfficeDel.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await CreateBooking(request);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseOfficeDel.StatusCode == 409 || (int)responseOfficeDel.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, office not found" });
                    }
                    if ((int)responseOfficeDel.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (OfficeService)" });
                    }
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }

                IConnection conn = _queueRabbitMQ.GetRabbitConnection();
                IModel channel = _queueRabbitMQ.GetRabbitChannel(conn);
                _queueRabbitMQ.SendMessage(conn, JsonSerializer.Serialize(new Report
                {
                    Model = car.Model,
                    Location = office.Location
                }));
                channel.Close();
                conn.Close();

                return new ServiceResponseBooking(201, $"{_configuration["ServerUrl"]}/booking/{bookingUid}");
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseBooking(500, e.Message);
            }
        }

        // Отмена брони
        public async Task<ServiceResponseBooking> DeleteBooking(Guid bookingUid)
        {
            try
            {
                Booking booking = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.Id == bookingUid)
                    .SingleOrDefaultAsync();

                if (booking == null)
                {
                    return new ServiceResponseBooking(404, new ErrorResponse { Message = "Booking not found" });
                }

                // Запрос для отмены брони, чтобы пометить что не было оплачено, либо были возвращены деньги
                var tokenPayment = _tokenService.GetPaymentServiceToken();
                if (string.IsNullOrEmpty(tokenPayment))
                    if (!await AuthorizePayment())
                        return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequest = new HttpRequestMessage(HttpMethod.Delete,
                    $"{_configuration["PaymentServiceUri"]}/payment/{booking.PaymentUid}");

                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetPaymentServiceToken());
                HttpResponseMessage response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizePayment())
                            return await DeleteBooking(bookingUid);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)response.StatusCode == 409 || (int)response.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, payment not found" });
                    }
                    if ((int)response.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (PaymentService)" });
                    }
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }

                booking.Status = BookingStatus.CANCELED;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                // Запрос к офисам для возврата автомобиля
                var tokenOffice = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(tokenOffice))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequestOffice = new HttpRequestMessage(HttpMethod.Post,
                    $"{_configuration["OfficeServiceUri"]}/office/return");

                httpRequestOffice.Content = new StringContent(JsonSerializer.Serialize(new ReturnCarToOffice
                {
                    OfficeUid = booking.TakeFromOfficeUid,
                    CarUid = booking.CarUid,
                    RegistrationNumber = booking.RegistrationNumber
                }));

                httpRequestOffice.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpClient clientOffice = _clientFactory.CreateClient();
                clientOffice.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage responseOffice = await clientOffice.SendAsync(httpRequestOffice);

                if (!responseOffice.IsSuccessStatusCode)
                {
                    if (responseOffice.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await DeleteBooking(bookingUid);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseOffice.StatusCode == 409 || (int)responseOffice.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, payment not found" });
                    }
                    if ((int)responseOffice.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (OfficeService)" });
                    }
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }

                return new ServiceResponseBooking(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseBooking(500, e.Message);
            }
        }

        // Закрыть бронь
        public async Task<ServiceResponseBooking> FinishBooking(Guid bookingUid)
        {
            try
            {
                Booking booking = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.Id == bookingUid)
                    .SingleOrDefaultAsync();

                if (booking == null)
                {
                    return new ServiceResponseBooking(404, new ErrorResponse { Message = "Booking not found" });
                }

                booking.Status = BookingStatus.FINISHED;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                // Запрос к офисам для возврата автомобиля
                var tokenOffice = _tokenService.GetOfficeServiceToken();
                if (string.IsNullOrEmpty(tokenOffice))
                    if (!await AuthorizeOffice())
                        return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequestOffice = new HttpRequestMessage(HttpMethod.Post,
                    $"{_configuration["OfficeServiceUri"]}/office/return");

                httpRequestOffice.Content = new StringContent(JsonSerializer.Serialize(new ReturnCarToOffice
                {
                    OfficeUid = booking.ReturnToOfficeUid,
                    CarUid = booking.CarUid,
                    RegistrationNumber = booking.RegistrationNumber
                }));

                httpRequestOffice.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpClient clientOffice = _clientFactory.CreateClient();
                clientOffice.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetOfficeServiceToken());
                HttpResponseMessage responseOffice = await clientOffice.SendAsync(httpRequestOffice);

                if (!responseOffice.IsSuccessStatusCode)
                {
                    if (responseOffice.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeOffice())
                            return await FinishBooking(bookingUid);
                        else
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseOffice.StatusCode == 409 || (int)responseOffice.StatusCode == 404)
                    {
                        return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, payment not found" });
                    }
                    if ((int)responseOffice.StatusCode == 500)
                    {
                        return new ServiceResponseBooking(503, new ErrorResponse { Message = "Internal Server Error (PaymentService)" });
                    }
                    return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                }

                return new ServiceResponseBooking(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseBooking(500, e.Message);
            }
        }

        // Получение информации о брони
        public async Task<ServiceResponseBooking> GetBookingInfo(Guid bookingUid)
        {
            try
            {
                Booking bookingItems = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.Id == bookingUid)
                    .SingleOrDefaultAsync();

                if (bookingItems == null)
                {
                    return new ServiceResponseBooking(404, new ErrorResponse { Message = "Booking info not found" });
                }

                var bookingInfo = new AllBookingInfo();

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;

                // Получение информации об автомобиле
                var responseAvailable = await SiteAvailable($"{_configuration["CarServiceUri"]}/car/{ bookingItems.CarUid}");
                if (responseAvailable.StatusCode != 500 && responseAvailable.StatusCode != 503)
                {
                    var tokenCar = _tokenService.GetCarServiceToken();
                    if (string.IsNullOrEmpty(tokenCar))
                        if (!await AuthorizeCar())
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                    var httpRequestCar = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["CarServiceUri"]}/car/{bookingItems.CarUid}");

                    HttpClient clientCar = _clientFactory.CreateClient();
                    clientCar.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetCarServiceToken());
                    HttpResponseMessage responseCar = await clientCar.SendAsync(httpRequestCar);

                    if (!responseCar.IsSuccessStatusCode)
                    {
                        if (responseCar.StatusCode == HttpStatusCode.Forbidden)//403
                        {
                            if (await AuthorizeCar())
                                return await GetBookingInfo(bookingUid);
                            else
                                return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                        }
                        if ((int)responseCar.StatusCode == 409 || (int)responseCar.StatusCode == 404)
                        {
                            return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, car not found" });
                        }
                        return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                    }

                    using var responseStreamCar = await responseCar.Content.ReadAsStreamAsync();
                    AutomobileResponse car = await JsonSerializer.DeserializeAsync
                        <AutomobileResponse>(responseStreamCar, options);

                    bookingInfo.Brand = car.Brand;
                    bookingInfo.Model = car.Model;
                    bookingInfo.Power = car.Power;
                    bookingInfo.CarType = car.CarType;
                }

                // Получение информации об офисе, из которого забирается автомобиль
                responseAvailable = await SiteAvailable($"{_configuration["OfficeServiceUri"]}/office/{ bookingItems.TakeFromOfficeUid}");
                if (responseAvailable.StatusCode != 500  && responseAvailable.StatusCode != 503)
                {
                    var tokenOffice = _tokenService.GetOfficeServiceToken();
                    if (string.IsNullOrEmpty(tokenOffice))
                        if (!await AuthorizeOffice())
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                    var httpRequestOffice = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["OfficeServiceUri"]}/office/{bookingItems.TakeFromOfficeUid}");

                    HttpClient clientOffice = _clientFactory.CreateClient();
                    clientOffice.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokenService.GetOfficeServiceToken());
                    HttpResponseMessage responseOffice = await clientOffice.SendAsync(httpRequestOffice);

                    if (!responseOffice.IsSuccessStatusCode)
                    {
                        if (responseOffice.StatusCode == HttpStatusCode.Forbidden)//403
                        {
                            if (await AuthorizeOffice())
                                return await GetBookingInfo(bookingUid);
                            else
                                return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                        }
                        if ((int)responseOffice.StatusCode == 409 || (int)responseOffice.StatusCode == 404)
                        {
                            return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, office not found" });
                        }
                        return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                    }

                    using var responseStreamOffice = await responseOffice.Content.ReadAsStreamAsync();
                    OfficeResponse office = await JsonSerializer.DeserializeAsync
                        <OfficeResponse>(responseStreamOffice, options);

                    OfficeResponse officeReturn;
                    // Если офис возвращения другой
                    if (bookingItems.TakeFromOfficeUid != bookingItems.ReturnToOfficeUid)
                    {
                        var httpRequestOfficeReturn = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["OfficeServiceUri"]}/office/{bookingItems.ReturnToOfficeUid}");

                        HttpClient clientOfficeReturn = _clientFactory.CreateClient();
                        clientOfficeReturn.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            _tokenService.GetOfficeServiceToken());
                        HttpResponseMessage responseOfficeReturn = await clientOfficeReturn.SendAsync(httpRequestOfficeReturn);

                        if (!responseOfficeReturn.IsSuccessStatusCode)
                        {
                            if (responseOfficeReturn.StatusCode == HttpStatusCode.Forbidden)//403
                            {
                                if (await AuthorizeOffice())
                                    return await GetBookingInfo(bookingUid);
                                else
                                    return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                            }
                            if ((int)responseOfficeReturn.StatusCode == 409 || (int)responseOfficeReturn.StatusCode == 404)
                            {
                                return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, office not found" });
                            }
                            return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                        }

                        using var responseStreamOfficeReturn = await responseOfficeReturn.Content.ReadAsStreamAsync();
                        officeReturn = await JsonSerializer.DeserializeAsync
                           <OfficeResponse>(responseStreamOfficeReturn, options);
                    }
                    else
                    {
                        officeReturn = office;
                    }

                    bookingInfo.LocationFrom = office.Location;
                    bookingInfo.LocationTo = officeReturn.Location;
                }
                
                // Получение информации об оплате
                responseAvailable = await SiteAvailable($"{_configuration["PaymentServiceUri"]}/payment/{ bookingItems.PaymentUid}");
                if (responseAvailable.StatusCode != 500 && responseAvailable.StatusCode != 503)
                {
                    var tokenPayment = _tokenService.GetPaymentServiceToken();
                    if (string.IsNullOrEmpty(tokenPayment))
                        if (!await AuthorizePayment())
                            return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                    var httpRequestPayment = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["PaymentServiceUri"]}/payment/{bookingItems.PaymentUid}");

                    HttpClient clientPayment = _clientFactory.CreateClient();
                    clientPayment.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetPaymentServiceToken());
                    HttpResponseMessage responsePayment = await clientPayment.SendAsync(httpRequestPayment);

                    if (!responsePayment.IsSuccessStatusCode)
                    {
                        if (responsePayment.StatusCode == HttpStatusCode.Forbidden)//403
                        {
                            if (await AuthorizePayment())
                                return await GetBookingInfo(bookingUid);
                            else
                                return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                        }
                        if ((int)responsePayment.StatusCode == 409 || (int)responsePayment.StatusCode == 404)
                        {
                            return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, payment not found" });
                        }
                        return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                    }

                    using var responseStreamPayment = await responsePayment.Content.ReadAsStreamAsync();
                    PaymentResponse payment = await JsonSerializer.DeserializeAsync
                        <PaymentResponse>(responseStreamPayment, options);

                    bookingInfo.PaymentUid = payment.Id;
                    bookingInfo.PaymentStatus = payment.Status;
                    bookingInfo.Price = payment.Price;
                }

                bookingInfo.RegistrationNumber = bookingItems.RegistrationNumber;
                bookingInfo.Status = bookingItems.Status;
                bookingInfo.AvailabilitySchedule = bookingItems.BookingPeriod;
                
                return new ServiceResponseBooking(bookingInfo);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseBooking(500, e.Message);
            }
        }
        
        // Получение информации обо всех бронях пользователя
        public async Task<ServiceResponseBooking> GetBookingsInfo(Guid userUid)
        {
            try
            {
                IEnumerable<Booking> bookings = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.UserUid == userUid)
                    .ToListAsync();

                if (bookings == null)
                {
                    return new ServiceResponseBooking(404, new ErrorResponse { Message = "Booking info not found" });
                }

                var paymentAvailable = false;
                var responseAvailable = await SiteAvailable($"{_configuration["PaymentServiceUri"]}/payment/00000000-0000-0000-0000-000000000000");
                if (responseAvailable.StatusCode != 500)
                {
                    paymentAvailable = true;
                }
                
                List<BookingsInfo> BookingResponse = new List<BookingsInfo>();
                foreach (Booking booking in bookings)
                {
                    if (paymentAvailable)
                    {
                        // Получение информации об оплате
                        var tokenPayment = _tokenService.GetPaymentServiceToken();
                        if (string.IsNullOrEmpty(tokenPayment))
                            if (!await AuthorizePayment())
                                return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });

                        var httpRequestPayment = new HttpRequestMessage(HttpMethod.Get, 
                            $"{_configuration["PaymentServiceUri"]}/payment/{booking.PaymentUid}");

                        HttpClient clientPayment = _clientFactory.CreateClient();
                        clientPayment.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                            _tokenService.GetPaymentServiceToken());
                        HttpResponseMessage responsePayment = await clientPayment.SendAsync(httpRequestPayment);

                        if (!responsePayment.IsSuccessStatusCode)
                        {
                            if (responsePayment.StatusCode == HttpStatusCode.Forbidden)//403
                            {
                                if (await AuthorizePayment())
                                    return await GetBookingsInfo(userUid);
                                else
                                    return new ServiceResponseBooking(401, new ErrorResponse { Message = "Unauthorized" });
                            }
                            if ((int)responsePayment.StatusCode == 409 || (int)responsePayment.StatusCode == 404)
                            {
                                return new ServiceResponseBooking(409, new ErrorResponse { Message = "Conflict, payment not found" });
                            }
                            return new ServiceResponseBooking(422, new ErrorResponse { Message = "External request failed" });
                        }

                        var options = new JsonSerializerOptions
                        {
                            Converters = { new JsonStringEnumConverter() }
                        };
                        options.PropertyNameCaseInsensitive = true;

                        using var responseStreamPayment = await responsePayment.Content.ReadAsStreamAsync();
                        PaymentResponse payment = await JsonSerializer.DeserializeAsync
                            <PaymentResponse>(responseStreamPayment, options);

                        BookingResponse.Add(new BookingsInfo
                        {
                            BookingUid = booking.Id,
                            RegistrationNumber = booking.RegistrationNumber,
                            Status = booking.Status,
                            PaymentUid = payment.Id,
                            PaymentStatus = payment.Status,
                            Price = payment.Price
                        });
                    }
                    else
                    {
                        BookingResponse.Add(new BookingsInfo
                        {
                            BookingUid = booking.Id,
                            RegistrationNumber = booking.RegistrationNumber,
                            Status = booking.Status
                        });
                    }
                    
                }
                return new ServiceResponseBooking(BookingResponse);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseBooking(500, e.Message);
            }
        }

        public async Task<ServiceResponseBooking> SiteAvailable(string Url)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{Url}");
                HttpClient client = _clientFactory.CreateClient();

                HttpResponseMessage response = await client.SendAsync(httpRequest);

                Console.WriteLine((int)response.StatusCode);
                return new ServiceResponseBooking((int)response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new ServiceResponseBooking(500);
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
    }
}
