using GatewayService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Interfaces
{
    public interface IGatewayService
    {
        Task<ServiceResponseGateway> GetCars();
        Task<ServiceResponseGateway> GetOfficeCars(Guid officeUid);
        Task<ServiceResponseGateway> GetOfficesCar(Guid carUid);
        Task<ServiceResponseGateway> GetOfficeCar(Guid officeuID, Guid carUid);
        Task<ServiceResponseGateway> GetOffices();
        Task<ServiceResponseGateway> GetBooking(Guid bookingUid);
        Task<ServiceResponseGateway> GetBookings(Guid userUid);
        Task<ServiceResponseGateway> CreateBooking(CreateBookingRequest request);
        Task<ServiceResponseGateway> GetUsers();
        Task<ServiceResponseGateway> AddNewUser(AddUserRequest request);
        Task<ServiceResponseGateway> GetReportsBookingByModel();
        Task<ServiceResponseGateway> GetReportsBookingByOffice();
        Task<ServiceResponseGateway> AddNewCar(AddCarRequest request);
        Task<ServiceResponseGateway> AddCarToOffice(Guid officeUid, Guid carUid);
        Task<ServiceResponseGateway> DeleteCar(Guid carUid);
        Task<ServiceResponseGateway> DeleteCarFromOffice(Guid officeUid, Guid carUid);
        Task<ServiceResponseGateway> PayBooking(Guid paymentUid);
        Task<ServiceResponseGateway> CancelBooking(Guid bookingUid);
        Task<ServiceResponseGateway> FinishBooking(Guid bookingUid);
    }
}
