using CarService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarService.Interfaces
{
    public interface ICarService
    {
        Task<ServiceResponseCar> GetAllCars();
        Task<ServiceResponseCar> AddNewCar(NewCar request);
        Task<ServiceResponseCar> DeleteCar(Guid carUid);
        Task<ServiceResponseCar> GetCar(Guid carUid);
    }
}
