using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum FinanceKind { Rent = 1, Purchase = 2, Bill = 3, Payroll = 4, Income = 5, Other = 6 }
public enum FinanceDirection { In = 1, Out = 2 }

public class FinancialTransaction : TenantEntity
{
    public FinanceKind Kind { get; set; }
    public FinanceDirection Direction { get; set; }
    public Money Amount { get; set; } = Money.Zero("IRR");
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? CounterpartyUserId { get; set; }
    public string? Note { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}