using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentService.Interfaces;
using PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Services
{
    public class PaymentsService : IPaymentService
    {
        private readonly PaymentContext _context;
        private readonly IConfiguration _configuration;

        public PaymentsService(PaymentContext context,
                               IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Создание оплаты
        public async Task<ServiceResponsePayment> CreatePayment()
        {
            var payment = new Payments();
            payment.Id = Guid.NewGuid();
            payment.Status = PaymentStatus.NEW;

            Random x = new Random();
            payment.Price = x.Next(1000, 9999);
            
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            return new ServiceResponsePayment(payment);
        }

        // Получение информации об оплате
        public async Task<ServiceResponsePayment> GetPaymentInfo(Guid paymentUid)
        {
            try
            {
                Payments payment = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Id == paymentUid)
                    .SingleOrDefaultAsync();

                if (payment == null)
                {
                    return new ServiceResponsePayment(404, new ErrorResponse { Message = "Payment info not found" });
                }

                return new ServiceResponsePayment(payment);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponsePayment(500, e.Message);
            }
        }

        // Отмена оплаты
        public async Task<ServiceResponsePayment> CancelPayment(Guid paymentUid)
        {
            try
            {
                Payments payment = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Id == paymentUid)
                    .SingleOrDefaultAsync();

                if (payment == null)
                {
                    return new ServiceResponsePayment(404, new ErrorResponse { Message = "Payment info not found" });
                }

                if (payment.Status == PaymentStatus.NEW)
                {
                    payment.Status = PaymentStatus.CANCELED;
                }
                else
                {
                    payment.Status = PaymentStatus.REVERSED;
                }

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                return new ServiceResponsePayment(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponsePayment(500, e.Message);
            }
        }

        // Оплата брони
        public async Task<ServiceResponsePayment> PayBooking(Guid paymentUid)
        {
            try
            {
                Payments payment = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Id == paymentUid)
                    .SingleOrDefaultAsync();

                if (payment == null)
                {
                    return new ServiceResponsePayment(404, new ErrorResponse { Message = "Payment info not found" });
                }

                payment.Status = PaymentStatus.PAID;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                return new ServiceResponsePayment(204);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponsePayment(500, e.Message);
            }
        }
    }
}
