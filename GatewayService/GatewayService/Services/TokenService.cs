using GatewayService.Interfaces;

namespace GatewayService.Services
{
    public class TokenService : ITokenService
    {
        private string _carServiceToken;
        private string _officeServiceToken;
        private string _paymentServiceToken;
        private string _bookingServiceToken;
        private string _sessionServiceToken;
        private string _reportServiceToken;

        public TokenService()
        {
            _carServiceToken = string.Empty;
            _officeServiceToken = string.Empty;
            _paymentServiceToken = string.Empty;
            _bookingServiceToken = string.Empty;
            _sessionServiceToken = string.Empty;
            _reportServiceToken = string.Empty;
        }

        public string GetBookingServiceToken()
        {
            return _bookingServiceToken;
        }

        public string GetCarServiceToken()
        {
            return _carServiceToken;
        }

        public string GetOfficeServiceToken()
        {
            return _officeServiceToken;
        }

        public string GetPaymentServiceToken()
        {
            return _paymentServiceToken;
        }

        public string GetReportServiceToken()
        {
            return _reportServiceToken;
        }

        public string GetSessionServiceToken()
        {
            return _sessionServiceToken;
        }

        public void SetBookingServiceToken(string token)
        {
            _bookingServiceToken = token;
        }

        public void SetCarServiceToken(string token)
        {
            _carServiceToken = token;
        }

        public void SetOfficeServiceToken(string token)
        {
            _officeServiceToken = token;
        }

        public void SetPaymentServiceToken(string token)
        {
            _paymentServiceToken = token;
        }

        public void SetReportServiceToken(string token)
        {
            _reportServiceToken = token;
        }

        public void SetSessionServiceToken(string token)
        {
            _sessionServiceToken = token;
        }
    }
}
