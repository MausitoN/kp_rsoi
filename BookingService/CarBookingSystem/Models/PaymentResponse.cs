using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Models
{
    public enum PaymentStatus
    {
        NEW,       // новое
        PAID,      // оплачено
        CANCELED,  // отмененное неоплаченное
        REVERSED   // отмена с возвратом денег
    }

    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public int Price { get; set; }
    }
}
