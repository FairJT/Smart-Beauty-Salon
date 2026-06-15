using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SalonOS.Shared;
using SalonOS.Shared.Payments;

namespace SalonOS.Infrastructure.Payments;

public class ZarinpalProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _merchantId;
    private readonly bool _isSandbox;

    private const string SandboxBase = "https://sandbox.zarinpal.com/pg/v4/payment/";
    private const string ProdBase = "https://api.zarinpal.com/pg/v4/payment/";
    private const string SandboxStartPay = "https://sandbox.zarinpal.com/pg/StartPay/";
    private const string ProdStartPay = "https://www.zarinpal.com/pg/StartPay/";

    private string BaseUrl => _isSandbox ? SandboxBase : ProdBase;
    private string StartPayUrl => _isSandbox ? SandboxStartPay : ProdStartPay;

    public ZarinpalProvider(HttpClient httpClient, string merchantId, string apiKey, bool isSandbox = false)
    {
        _httpClient = httpClient;
        _merchantId = merchantId;
        _isSandbox = isSandbox;
    }

    public async Task<PaymentSession> CreatePaymentAsync(CreatePaymentInput input, string idempotencyKey)
    {
        var requestBody = new Dictionary<string, object>
        {
            ["merchant_id"] = _merchantId,
            ["amount"] = input.Amount.Amount,
            ["description"] = input.Description,
            ["callback_url"] = input.CallbackUrl,
            ["metadata"] = input.Metadata,
        };

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}request.json", requestBody);
        var json = await response.Content.ReadFromJsonAsync<ZarinpalResponse>();

        if (json?.Data == null || json.Data.Code != 100)
        {
            var errorMsg = json?.Errors?.FirstOrDefault()?.Message ?? "Zarinpal payment creation failed";
            throw new PaymentException(errorMsg);
        }

        var authority = json.Data.Authority;
        return new PaymentSession
        {
            Reference = authority,
            PaymentUrl = $"{StartPayUrl}{authority}",
            Amount = input.Amount,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
        };
    }

    public async Task<PaymentResult> VerifyPaymentAsync(string reference)
    {
        var requestBody = new Dictionary<string, object>
        {
            ["merchant_id"] = _merchantId,
            ["amount"] = 0,
            ["authority"] = reference,
        };

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}verify.json", requestBody);
        var json = await response.Content.ReadFromJsonAsync<ZarinpalResponse>();

        if (json?.Data == null)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                Reference = reference,
                ErrorMessage = json?.Errors?.FirstOrDefault()?.Message ?? "Verification failed",
            };
        }

        var isSuccess = json.Data.Code == 100;
        return new PaymentResult
        {
            IsSuccess = isSuccess,
            Reference = reference,
            Amount = Money.Of(json.Data.Amount, "IRR"),
            TransactionId = isSuccess ? json.Data.RefId : null,
            ErrorMessage = isSuccess ? null : $"Code: {json.Data.Code} - {json.Data.Message}",
        };
    }

    public WebhookEvent VerifyWebhook(ReadOnlySpan<byte> payload, string signature)
    {
        var json = Encoding.UTF8.GetString(payload);
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();

        var authority = data.GetValueOrDefault("Authority") ?? signature;
        var status = data.GetValueOrDefault("Status");

        return new WebhookEvent
        {
            EventType = status == "OK" ? "payment.verified" : "payment.failed",
            Reference = authority,
            Metadata = data,
        };
    }

    private class ZarinpalResponse
    {
        [JsonPropertyName("data")]
        public ZarinpalData? Data { get; set; }

        [JsonPropertyName("errors")]
        public List<ZarinpalError>? Errors { get; set; }
    }

    private class ZarinpalData
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("authority")]
        public string Authority { get; set; } = string.Empty;

        [JsonPropertyName("fee")]
        public long Fee { get; set; }

        [JsonPropertyName("fee_type")]
        public string FeeType { get; set; } = string.Empty;

        [JsonPropertyName("ref_id")]
        public string RefId { get; set; } = string.Empty;

        [JsonPropertyName("card_hash")]
        public string CardHash { get; set; } = string.Empty;

        [JsonPropertyName("card_pan")]
        public string CardPan { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }

    private class ZarinpalError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}

public class PaymentException : Exception
{
    public PaymentException(string message) : base(message) { }
}
