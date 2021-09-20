using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Models
{
    public enum PaymentStatus
    {
        NEW,       // новое
        PAID,      // оплачено
        CANCELED,  // отмененное неоплаченное
        REVERSED   // отмена с возвратом денег
    }
    public class Payments
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public int Price { get; set; }
    }
}
