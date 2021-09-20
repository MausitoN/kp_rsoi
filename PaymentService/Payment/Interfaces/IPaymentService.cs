using PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResponsePayment> CreatePayment();
        Task<ServiceResponsePayment> GetPaymentInfo(Guid paymentUid);
        Task<ServiceResponsePayment> CancelPayment(Guid paymentUid);
        Task<ServiceResponsePayment> PayBooking(Guid paymentUid);
    }
}
