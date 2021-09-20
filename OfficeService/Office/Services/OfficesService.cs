using Microsoft.EntityFrameworkCore;
using Office;
using OfficeService.Interfaces;
using OfficeService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using ShareService.Configurations;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.Extensions.Options;

namespace OfficeService.Services
{
    public class OfficesService : IOfficeService
    {
        private readonly OfficeContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ServiceCredentials _serviceCredentials;

        public OfficesService(OfficeContext context,
                                              IHttpClientFactory clientFactory,
                                              IConfiguration configuration,
                                              ITokenService tokenService,
                                              IOptions<ServiceCredentials> secretOptions)
        {
            _context = context;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _serviceCredentials = secretOptions.Value;
        }

        // Получение информации о всех офисах
        public async Task<ServiceResponseOffice> GetAllOffices()
        {
            try
            {
                IEnumerable<RentOffice> offices = await _context.RentOffices
                    .AsNoTracking()
                    .ToListAsync();

                if (offices == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office info not found" });
                }

                return new ServiceResponseOffice(offices);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Получить информацию о конкретном офисе
        public async Task<ServiceResponseOffice> GetOffice(Guid officeUid)
        {
            try
            {
                RentOffice office = await _context.RentOffices
                    .AsNoTracking()
                    .Where(o => o.Id == officeUid)
                    .SingleOrDefaultAsync();

                if (office == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office info not found" });
                }

                return new ServiceResponseOffice(office);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Получить информацию обо всех автомобилях в конретном офисе
        public async Task<ServiceResponseOffice> GetAllCarsOffice(Guid officeUid)
        {
            try
            {
                RentOffice office = await _context.RentOffices
                    .AsNoTracking()
                    .Where(o => o.Id == officeUid)
                    .SingleOrDefaultAsync();

                if (office == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office not found" });
                }

                IEnumerable<AvailableCar> cars = await _context.AvailableCars
                    .AsNoTracking()
                    .Where(ac => ac.OfficeId == officeUid)
                    .ToListAsync();

                if (cars == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Cars not found" });
                }

                List<AllCarInOffice> CarResponse = new List<AllCarInOffice>();

                foreach (AvailableCar car in cars)
                {
                    var tokenCar = _tokenService.GetCarServiceToken();
                    if (string.IsNullOrEmpty(tokenCar))
                        if (!await AuthorizeCar())
                            return new ServiceResponseOffice(401, new ErrorResponse { Message = "Unauthorized" });

                    var httpRequestWarehouse = new HttpRequestMessage(HttpMethod.Get,
                            $"{_configuration["CarServiceUri"]}/car/{car.CarUid}");

                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    options.PropertyNameCaseInsensitive = true;
                    HttpClient clientCar = _clientFactory.CreateClient();
                    clientCar.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetCarServiceToken());
                    HttpResponseMessage responseCar = await clientCar.SendAsync(httpRequestWarehouse);

                    if (!responseCar.IsSuccessStatusCode)
                    {
                        if (responseCar.StatusCode == HttpStatusCode.Forbidden)//403
                        {
                            if (await AuthorizeCar())
                                return await GetAllCarsOffice(officeUid);
                            else
                                return new ServiceResponseOffice(401, new ErrorResponse { Message = "Unauthorized" });
                        }
                        if ((int)responseCar.StatusCode == 409 || (int)responseCar.StatusCode == 404)
                        {
                            return new ServiceResponseOffice(409, new ErrorResponse { Message = "Conflict, car not found" });
                        }
                        if ((int)responseCar.StatusCode == 500)
                        {
                            return new ServiceResponseOffice(503, new ErrorResponse { Message = "Internal Server Error (CarService)" });
                        }
                        return new ServiceResponseOffice(422, new ErrorResponse { Message = "External request failed" });
                    }

                    using var responseStream = await responseCar.Content.ReadAsStreamAsync();
                    CarInfo item = await JsonSerializer.DeserializeAsync
                        <CarInfo>(responseStream, options);

                    CarResponse.Add(new AllCarInOffice
                    {
                        OfficeUid = office.Id,
                        LocationOffice = office.Location,
                        CarUid = car.CarUid,
                        RegistrationNumber = car.RegistrationNumber,
                        AvailabilityScheduleFirst = car.AvailabilityScheduleFirst,
                        AvailabilityScheduleSecond = car.AvailabilityScheduleSecond,
                        Brand = item.Brand,
                        Model = item.Model,
                        Power = item.Power,
                        CarType = item.CarType
                    });
                }

                return new ServiceResponseOffice(CarResponse);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Получить информацию о конкретном автомобиле в конкретном офисе
        public async Task<ServiceResponseOffice> GetCarOffice(Guid officeUid, Guid carUid)
        {
            try
            {
                AvailableCar car = await _context.AvailableCars
                    .AsNoTracking()
                    .Where(ac => ac.OfficeId == officeUid && ac.CarUid == carUid)
                    .SingleOrDefaultAsync();

                if (car == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Available car not found" });
                }

                RentOffice office = await _context.RentOffices
                    .AsNoTracking()
                    .Where(o => o.Id == officeUid)
                    .SingleOrDefaultAsync();

                if (office == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office not found" });
                }

                var tokenCar = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(tokenCar))
                    if (!await AuthorizeCar())
                        return new ServiceResponseOffice(401, new ErrorResponse { Message = "Unauthorized" });

                var httpRequestWarehouse = new HttpRequestMessage(HttpMethod.Get,
                       $"{_configuration["CarServiceUri"]}/car/{carUid}");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient clientCar = _clientFactory.CreateClient();
                clientCar.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetCarServiceToken());
                HttpResponseMessage responseCar = await clientCar.SendAsync(httpRequestWarehouse);

                if (!responseCar.IsSuccessStatusCode)
                {
                    if (responseCar.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeCar())
                            return await GetCarOffice(officeUid, carUid);
                        else
                            return new ServiceResponseOffice(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseCar.StatusCode == 409 || (int)responseCar.StatusCode == 404)
                    {
                        return new ServiceResponseOffice(409, new ErrorResponse { Message = "Conflict, car not found" });
                    }
                    if ((int)responseCar.StatusCode == 500)
                    {
                        return new ServiceResponseOffice(503, new ErrorResponse { Message = "Internal Server Error (CarService)" });
                    }
                    return new ServiceResponseOffice(422, new ErrorResponse { Message = "External request failed" });
                }

                using var responseStream = await responseCar.Content.ReadAsStreamAsync();
                AllCarInOffice item = await JsonSerializer.DeserializeAsync
                    <AllCarInOffice>(responseStream, options);

                var availableCar = new AllCarInOffice();
                availableCar.OfficeUid = office.Id;
                availableCar.LocationOffice = office.Location;
                availableCar.CarUid = car.CarUid;
                availableCar.RegistrationNumber = car.RegistrationNumber;
                availableCar.AvailabilityScheduleFirst = car.AvailabilityScheduleFirst;
                availableCar.AvailabilityScheduleSecond = car.AvailabilityScheduleSecond;
                availableCar.Brand = item.Brand;
                availableCar.Model = item.Model;
                availableCar.Power = item.Power;
                availableCar.CarType = item.CarType;

                return new ServiceResponseOffice(availableCar);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Получить информацию о всех офисах, в которых находится конкретный автомобиль
        public async Task<ServiceResponseOffice> GetCarAllOffice(Guid carUid)
        {
            try
            {
                IEnumerable<AvailableCar> cars = await _context.AvailableCars
                    .AsNoTracking()
                    .Where(ac => ac.CarUid == carUid)
                    .ToListAsync();

                if (cars == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Available car not found" });
                }

                //Запрос на получение данных об автомобиле
                var tokenCar = _tokenService.GetCarServiceToken();
                if (string.IsNullOrEmpty(tokenCar))
                    if (!await AuthorizeCar())
                        return new ServiceResponseOffice(401, new ErrorResponse { Message = "Unauthorized" });
                
                var httpRequestCar = new HttpRequestMessage(HttpMethod.Get,
                        $"{_configuration["CarServiceUri"]}/car/{carUid}");

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                options.PropertyNameCaseInsensitive = true;
                HttpClient clientCar = _clientFactory.CreateClient();
                clientCar.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        _tokenService.GetCarServiceToken());
                HttpResponseMessage responseCar = await clientCar.SendAsync(httpRequestCar);

                if (!responseCar.IsSuccessStatusCode)
                {
                    if (responseCar.StatusCode == HttpStatusCode.Forbidden)//403
                    {
                        if (await AuthorizeCar())
                            return await GetCarAllOffice(carUid);
                        else
                            return new ServiceResponseOffice(401, new ErrorResponse { Message = "Unauthorized" });
                    }
                    if ((int)responseCar.StatusCode == 409 || (int)responseCar.StatusCode == 404)
                    {
                        return new ServiceResponseOffice(409, new ErrorResponse { Message = "Conflict, car not found" });
                    }
                    if ((int)responseCar.StatusCode == 500)
                    {
                        return new ServiceResponseOffice(503, new ErrorResponse { Message = "Internal Server Error (CarService)" });
                    }
                    return new ServiceResponseOffice(422, new ErrorResponse { Message = "External request failed" });
                }

                using var responseStream = await responseCar.Content.ReadAsStreamAsync();
                CarInfo item = await JsonSerializer.DeserializeAsync
                    <CarInfo>(responseStream, options);

                List<AllCarInOffice> OfficeResponse = new List<AllCarInOffice>();

                foreach (AvailableCar car in cars)
                {
                    RentOffice office = await _context.RentOffices
                    .AsNoTracking()
                    .Where(ro => ro.Id == car.OfficeId)
                    .SingleOrDefaultAsync();

                    if (office == null)
                    {
                        return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office not found" });
                    }

                    OfficeResponse.Add(new AllCarInOffice
                    {
                        OfficeUid = office.Id,
                        LocationOffice = office.Location,
                        CarUid = car.CarUid,
                        RegistrationNumber = car.RegistrationNumber,
                        AvailabilityScheduleFirst = car.AvailabilityScheduleFirst,
                        AvailabilityScheduleSecond = car.AvailabilityScheduleSecond,
                        Brand = item.Brand,
                        Model = item.Model,
                        Power = item.Power,
                        CarType = item.CarType
                    });
                }

                return new ServiceResponseOffice(OfficeResponse);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Добавление автомобиля в офис
        public async Task<ServiceResponseOffice> AddCarToOffice(Guid officeUid, Guid carUid)
        {
            try
            {
                RentOffice office = await _context.RentOffices
                    .AsNoTracking()
                    .Where(ro => ro.Id == officeUid)
                    .SingleOrDefaultAsync();

                if (office == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office not found" });
                }

                AvailableCar checkAvailableCar = await _context.AvailableCars
                    .AsNoTracking()
                    .Where(ac => ac.OfficeId == officeUid && ac.CarUid == carUid)
                    .SingleOrDefaultAsync();
                
                if (checkAvailableCar != null)
                {
                    return new ServiceResponseOffice(409, new ErrorResponse { Message = "This automobile is already in that office" });
                }

                var availableCar = new AvailableCar();
                availableCar.CarUid = carUid;

                // Генерация регистрационного номера и расписания доступности
                Random x = new Random();
                availableCar.RegistrationNumber = x.Next(100000, 999999);
                availableCar.AvailabilityScheduleFirst = DateTime.Today;//DateTime.Parse("2010-01-01"); 
                availableCar.AvailabilityScheduleSecond = DateTime.Today.AddDays(10);
                availableCar.OfficeId = officeUid;

                await _context.AvailableCars.AddAsync(availableCar);
                await _context.SaveChangesAsync();

                return new ServiceResponseOffice(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Возвращение автомобиля в офис
        public async Task<ServiceResponseOffice> ReturnCarToOffice(ReturnCarRequest request)
        {
            try
            {
                RentOffice office = await _context.RentOffices
                    .AsNoTracking()
                    .Where(ro => ro.Id == request.OfficeUid)
                    .SingleOrDefaultAsync();

                if (office == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Office not found" });
                }

                AvailableCar checkAvailableCar = await _context.AvailableCars
                    .AsNoTracking()
                    .Where(ac => ac.OfficeId == request.OfficeUid && ac.CarUid == request.CarUid)
                    .SingleOrDefaultAsync();

                if (checkAvailableCar != null)
                {
                    return new ServiceResponseOffice(409, new ErrorResponse { Message = "This automobile is already in that office" });
                }

                var availableCar = new AvailableCar();
                availableCar.CarUid = request.CarUid;

                // Генерация регистрационного номера и расписания доступности
                Random x = new Random();
                availableCar.RegistrationNumber = long.Parse(request.RegistrationNumber);
                availableCar.AvailabilityScheduleFirst = DateTime.Today; 
                availableCar.AvailabilityScheduleSecond = DateTime.Today.AddDays(10);
                availableCar.OfficeId = request.OfficeUid;

                await _context.AvailableCars.AddAsync(availableCar);
                await _context.SaveChangesAsync();

                return new ServiceResponseOffice(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
            }
        }

        // Удаление автомобиля из офиса
        public async Task<ServiceResponseOffice> DeleteCarFromOffice(Guid officeUid, Guid carUid)
        {
            try
            {
                AvailableCar car = await _context.AvailableCars
                    .AsNoTracking()
                    .Where(ac => ac.OfficeId == officeUid && ac.CarUid == carUid)
                    .SingleOrDefaultAsync();

                if (car == null)
                {
                    return new ServiceResponseOffice(404, new ErrorResponse { Message = "Available car not found" });
                }

                _context.AvailableCars.Remove(car);
                await _context.SaveChangesAsync();

                return new ServiceResponseOffice(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseOffice(500, e.Message);
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
    }
}
