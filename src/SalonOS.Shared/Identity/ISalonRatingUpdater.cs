namespace SalonOS.Shared.Identity
{
    /// <summary>
    /// Lets other modules keep a salon's denormalized rating aggregate in sync
    /// without reaching into the Identity tables directly.
    /// </summary>
    public interface ISalonRatingUpdater
    {
        Task AddRatingAsync(Guid tenantId, int rating);
    }
}