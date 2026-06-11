using RestSharp;

namespace SmartSalon.Services
{
    public interface ISmsService
    {
        Task<bool> SendAsync(string mobile, string message);
        Task<bool> SendAppointmentReminderAsync(string mobile, string clientName,
            string salonName, string artistName, string dateTime);
        Task<bool> SendAppointmentConfirmedAsync(string mobile, string clientName,
            string salonName, string dateTime);
        Task<bool> SendAppointmentCancelledAsync(string mobile, string clientName,
            string salonName);
    }

    public class SmsService : ISmsService
    {
        private readonly string _apiKey;
        private readonly string _senderNumber;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration config, ILogger<SmsService> logger)
        {
            _apiKey = config["MeliPayamak:ApiKey"] ?? "";
            _senderNumber = config["MeliPayamak:Sender"] ?? "";
            _logger = logger;
        }

        public async Task<bool> SendAsync(string mobile, string message)
        {
            try
            {
                var parts = _apiKey.Split(':');
                var username = parts[0];
                var password = parts[1];

                var client = new RestClient("https://rest.payamak-panel.com");
                var request = new RestRequest("/api/SendSMS/SendSMS", Method.Post);

                request.AddParameter("username", username);
                request.AddParameter("password", password);
                request.AddParameter("to", mobile);
                request.AddParameter("from", _senderNumber);
                request.AddParameter("text", message);
                request.AddParameter("isFlash", "false");
                request.AddParameter("udh", "");
                request.AddParameter("recId", "");
                request.AddParameter("status", "");
                request.AddParameter("filterId", "");

                var response = await client.ExecuteAsync(request);

                _logger.LogInformation("SMS sent to {mobile}: {status}",
                    mobile, response.StatusCode);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS sending failed to {mobile}", mobile);
                return false;
            }
        }

        public async Task<bool> SendAppointmentReminderAsync(
            string mobile, string clientName, string salonName,
            string artistName, string dateTime)
        {
            var message = $"سلام {clientName} عزیز،\n" +
                          $"یادآوری نوبت شما در {salonName}\n" +
                          $"هنرمند: {artistName}\n" +
                          $"زمان: {dateTime}\n" +
                          $"سالن هوشمند ابری";

            return await SendAsync(mobile, message);
        }

        public async Task<bool> SendAppointmentConfirmedAsync(
            string mobile, string clientName, string salonName, string dateTime)
        {
            var message = $"سلام {clientName} عزیز،\n" +
                          $"نوبت شما در {salonName}\n" +
                          $"برای {dateTime}\n" +
                          $"تایید شد ✅\n" +
                          $"سالن هوشمند ابری";

            return await SendAsync(mobile, message);
        }

        public async Task<bool> SendAppointmentCancelledAsync(
            string mobile, string clientName, string salonName)
        {
            var message = $"سلام {clientName} عزیز،\n" +
                          $"متاسفانه نوبت شما در {salonName}\n" +
                          $"لغو شد ❌\n" +
                          $"برای رزرو مجدد اقدام فرمایید.\n" +
                          $"سالن هوشمند ابری";

            return await SendAsync(mobile, message);
        }
    }
}