namespace SalonOS.Shared.Payments;

/// <summary>
/// Payment provider interface.
/// Domain code depends only on this interface; gateway SDKs live in adapters.
/// </summary>
public interface IPaymentProvider
{
    Task<PaymentSession> CreatePaymentAsync(CreatePaymentInput input, string idempotencyKey);
    Task<PaymentResult> VerifyPaymentAsync(string reference);
    WebhookEvent VerifyWebhook(ReadOnlySpan<byte> payload, string signature);
}

/// <summary>
/// Payment session created by the provider.
/// </summary>
public class PaymentSession
{
    public string Reference { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public Money Amount { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Payment result after verification.
/// </summary>
public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string Reference { get; set; } = string.Empty;
    public Money Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Webhook event from payment provider.
/// </summary>
public class WebhookEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public Money? Amount { get; set; }
    public string? TransactionId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Input for creating a payment.
/// </summary>
public class CreatePaymentInput
{
    public Money Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
