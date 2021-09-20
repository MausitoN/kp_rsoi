using CarService.Interfaces;
using CarService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CarService.Services
{
    public class CarsService : ICarService
    {
        private readonly CarContext _context;
        private readonly IConfiguration _configuration;

        public CarsService(CarContext context,
                           IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Получение списка всех автомобилейы
        public async Task<ServiceResponseCar> GetAllCars()
        {
            try
            {
                IEnumerable<Automobile> cars = await _context.Cars
                    .AsNoTracking()
                    .ToListAsync();

                if (cars == null)
                {
                    return new ServiceResponseCar(404, new ErrorResponse { Message = "Cars info not found" });
                }

                return new ServiceResponseCar(cars);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseCar(500, e.Message);
            }
        }

        // Добавление нового автомобиля
        public async Task<ServiceResponseCar> AddNewCar(NewCar request)
        {
            try
            {
                if (request == null)
                {
                    return new ServiceResponseCar(400, new ErrorResponse { Message = "Bad request format" });
                }

                Guid carUid = Guid.NewGuid();

                var car = new Automobile();
                car.Id = carUid;
                car.Brand = request.Brand;
                car.Model = request.Model;
                car.Power = request.Power;
                car.CarType = request.CarType;

                await _context.Cars.AddAsync(car);
                await _context.SaveChangesAsync();

                return new ServiceResponseCar(201, null, $"{_configuration["ServerUrl"]}/car/{carUid}");
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseCar(500, e.Message);
            }
        }

        // Удаление автомобиля
        public async Task<ServiceResponseCar> DeleteCar(Guid carUid)
        {
            try
            {
                Automobile car = await _context.Cars
                    .AsNoTracking()
                    .Where(a => a.Id == carUid)
                    .SingleOrDefaultAsync();

                if (car == null)
                {
                    return new ServiceResponseCar(404, new ErrorResponse { Message = "Car info not found" });
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                return new ServiceResponseCar(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseCar(500, e.Message);
            }
        }

        // Получение информации о конкретном автомобиле
        public async Task<ServiceResponseCar> GetCar(Guid carUid)
        {
            try
            {
                Automobile car = await _context.Cars
                    .AsNoTracking()
                    .Where(a => a.Id == carUid)
                    .SingleOrDefaultAsync();

                if (car == null)
                {
                    return new ServiceResponseCar(404, new ErrorResponse { Message = "Car info not found" });
                }
                return new ServiceResponseCar(car);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseCar(500, e.Message);
            }
        }
    }
}
