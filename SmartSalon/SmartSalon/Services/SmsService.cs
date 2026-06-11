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
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public SmsService(IConfiguration config, ILogger<SmsService> logger)
        {
            _apiKey = config["MeliPayamak:ApiKey"] ?? "";
            _senderNumber = config["MeliPayamak:Sender"] ?? "";
            _logger = logger;
        }

        public async Task<bool> SendAsync(string mobile, string message)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("USERNAME"))
            {
                _logger.LogWarning("SMS API not configured, skipping send to {mobile}", mobile);
                return false;
            }

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var parts = _apiKey.Split(':');
                    if (parts.Length < 2)
                    {
                        _logger.LogError("Invalid SMS API key format");
                        return false;
                    }

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

                    if (response.IsSuccessful)
                    {
                        _logger.LogInformation("SMS sent to {mobile} (attempt {attempt})",
                            mobile, attempt);
                        return true;
                    }

                    _logger.LogWarning("SMS attempt {attempt} failed to {mobile}: {status}",
                        attempt, mobile, response.StatusCode);

                    if (attempt < MaxRetries)
                        await Task.Delay(RetryDelayMs * attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SMS attempt {attempt} failed to {mobile}",
                        attempt, mobile);

                    if (attempt < MaxRetries)
                        await Task.Delay(RetryDelayMs * attempt);
                }
            }

            return false;
        }

        public async Task<bool> SendAppointmentReminderAsync(
            string mobile, string clientName, string salonName,
            string artistName, string dateTime)
        {
            var message = $"Hi {clientName},\n" +
                          $"Reminder: your appointment at {salonName}\n" +
                          $"Artist: {artistName}\n" +
                          $"Time: {dateTime}\n" +
                          $"SmartSalon";

            return await SendAsync(mobile, message);
        }

        public async Task<bool> SendAppointmentConfirmedAsync(
            string mobile, string clientName, string salonName, string dateTime)
        {
            var message = $"Hi {clientName},\n" +
                          $"Your appointment at {salonName}\n" +
                          $"for {dateTime}\n" +
                          $"has been confirmed.\n" +
                          $"SmartSalon";

            return await SendAsync(mobile, message);
        }

        public async Task<bool> SendAppointmentCancelledAsync(
            string mobile, string clientName, string salonName)
        {
            var message = $"Hi {clientName},\n" +
                          $"Unfortunately your appointment at {salonName}\n" +
                          $"has been cancelled.\n" +
                          $"Please book again.\n" +
                          $"SmartSalon";

            return await SendAsync(mobile, message);
        }
    }
}
