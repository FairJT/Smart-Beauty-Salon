using SalonOS.Shared;
using SalonOS.Shared.Payments;

namespace SalonOS.Infrastructure.Payments;

/// <summary>
/// Payment integration helper.
/// Provides methods for integrating payments into business flows.
/// </summary>
public static class PaymentIntegration
{
    /// <summary>
    /// Creates a payment for booking deposit.
    /// </summary>
    public static async Task<PaymentSession> CreateBookingDepositAsync(
        IPaymentProvider provider,
        Guid bookingId,
        Money depositAmount,
        string callbackUrl)
    {
        var input = new CreatePaymentInput
        {
            Amount = depositAmount,
            Description = $"Booking deposit for {bookingId}",
            CallbackUrl = callbackUrl,
            Metadata = new Dictionary<string, string>
            {
                ["bookingId"] = bookingId.ToString(),
                ["type"] = "booking_deposit"
            }
        };

        var idempotencyKey = $"booking_deposit_{bookingId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        return await provider.CreatePaymentAsync(input, idempotencyKey);
    }

    /// <summary>
    /// Creates a payment for package purchase.
    /// </summary>
    public static async Task<PaymentSession> CreatePackagePurchaseAsync(
        IPaymentProvider provider,
        Guid tenantId,
        Guid packageListingId,
        Money amount,
        string callbackUrl)
    {
        var input = new CreatePaymentInput
        {
            Amount = amount,
            Description = $"Package purchase for tenant {tenantId}",
            CallbackUrl = callbackUrl,
            Metadata = new Dictionary<string, string>
            {
                ["tenantId"] = tenantId.ToString(),
                ["packageListingId"] = packageListingId.ToString(),
                ["type"] = "package_purchase"
            }
        };

        var idempotencyKey = $"package_purchase_{tenantId}_{packageListingId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        return await provider.CreatePaymentAsync(input, idempotencyKey);
    }

    /// <summary>
    /// Verifies a payment and returns the result.
    /// </summary>
    public static async Task<PaymentResult> VerifyPaymentAsync(
        IPaymentProvider provider,
        string reference)
    {
        return await provider.VerifyPaymentAsync(reference);
    }
}
