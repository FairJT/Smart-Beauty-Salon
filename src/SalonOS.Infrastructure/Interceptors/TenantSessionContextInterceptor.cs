using Microsoft.EntityFrameworkCore.Diagnostics;
using SalonOS.Shared;
using System.Data.Common;

namespace SalonOS.Infrastructure.Interceptors;

/// <summary>
/// EF Core connection interceptor that runs sp_set_session_context immediately
/// after a connection is opened, feeding the RLS function (§R6.6, Task 8.2).
///
/// Sets two session-context keys:
///   N'TenantId'        — the current request's tenant GUID
///   N'IsPlatformOwner' — 1 when the user is PlatformOwner, 0 otherwise
///
/// This value comes from ITenantContext (which reads from JWT claims via ICurrentUser).
/// It is NEVER read from request body or query string (R3).
///
/// Register with DbContextOptionsBuilder.AddInterceptors() in Program.cs.
/// </summary>
public sealed class TenantSessionContextInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenant;

    public TenantSessionContextInterceptor(ITenantContext tenant) => _tenant = tenant;

    // ── Synchronous path ──────────────────────────────────────────────────────
    public override void ConnectionOpened(
        DbConnection connection, ConnectionEndEventData eventData)
    {
        SetSessionContext(connection);
        base.ConnectionOpened(connection, eventData);
    }

    // ── Asynchronous path ─────────────────────────────────────────────────────
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetSessionContext(DbConnection connection)
    {
        using var cmd = connection.CreateCommand();
        ApplySessionContext(cmd);
        cmd.ExecuteNonQuery();
    }

    private async Task SetSessionContextAsync(
        DbConnection connection, CancellationToken ct)
    {
        await using var cmd = connection.CreateCommand();
        ApplySessionContext(cmd);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private void ApplySessionContext(DbCommand cmd)
    {
        // Two separate calls — sp_set_session_context supports one key per call.
        cmd.CommandText = """
            EXEC sp_set_session_context N'TenantId', @tenantId, @readonly = 0;
            EXEC sp_set_session_context N'IsPlatformOwner', @isPlatformOwner, @readonly = 0;
            """;

        var tenantParam = cmd.CreateParameter();
        tenantParam.ParameterName = "@tenantId";
        tenantParam.Value = _tenant.TenantId == Guid.Empty
            ? DBNull.Value          // unauthenticated / no tenant → RLS blocks all rows
            : (object)_tenant.TenantId;
        cmd.Parameters.Add(tenantParam);

        var ownerParam = cmd.CreateParameter();
        ownerParam.ParameterName = "@isPlatformOwner";
        ownerParam.Value = _tenant.IsPlatformOwner ? 1 : 0;
        cmd.Parameters.Add(ownerParam);
    }
}
