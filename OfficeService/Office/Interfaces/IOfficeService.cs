using OfficeService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Interfaces
{
    public interface IOfficeService
    {
        Task<ServiceResponseOffice> GetAllOffices();
        Task<ServiceResponseOffice> GetOffice(Guid officeUid);
        Task<ServiceResponseOffice> GetAllCarsOffice(Guid officeUid);
        Task<ServiceResponseOffice> GetCarOffice(Guid officeUid, Guid carUid);
        Task<ServiceResponseOffice> GetCarAllOffice(Guid carUid);
        Task<ServiceResponseOffice> AddCarToOffice(Guid officeUid, Guid carUid);
        Task<ServiceResponseOffice> DeleteCarFromOffice(Guid officeUid, Guid carUid);
        Task<ServiceResponseOffice> ReturnCarToOffice(ReturnCarRequest request);
    }
}
