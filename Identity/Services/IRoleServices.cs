using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Services;

public interface IRoleServices
{
    Task CreateRole(RoleModel role, CancellationToken ct);
    Task<IdentityRole> GetRole(string roleName, CancellationToken ct);
}