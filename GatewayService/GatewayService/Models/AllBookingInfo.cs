using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Models
{
    public enum BookingStatus
    {
        NEW,      // новое
        FINISHED, // завершенное
        CANCELED, // отмененное
        EXPIRED   // истекшее
    }
    public enum PaymentStatus
    {
        NEW,       // новое
        PAID,      // оплачено
        CANCELED,  // отмененное неоплаченное
        REVERSED   // отмена с возвратом денег
    }
    public class AllBookingInfo
    {
        public string RegistrationNumber { get; set; }
        public BookingStatus Status { get; set; }
        public string AvailabilitySchedule { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Power { get; set; }
        public CarType CarType { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public Guid PaymentUid { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int Price { get; set; }
    }
}
