namespace SalonOS.Shared;

public abstract class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    // ── Audit columns ──────────────────────────────────────────
    public DateTime? UpdatedAt { get; set; }

    // ── Soft-delete ─────────────────────────────────────────────
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
