using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SalonOS.Api.Authorization;

/// <summary>
/// Dynamically builds an AuthorizationPolicy for any policy name starting with
/// "perm:" — e.g. "perm:appointment.cancel.all". Falls back to the default
/// provider for all other policy names. See §R6.1.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        const string prefix = "perm:";
        if (policyName.StartsWith(prefix, StringComparison.Ordinal))
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName[prefix.Length..]))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();
}
