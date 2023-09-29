using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Services;

public class RoleServices : IRoleServices
{
    private RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Constructor with the RoleManager dependency injected
    /// </summary>
    /// <param name="roleManager"></param>
    public RoleServices(
        RoleManager<IdentityRole> roleManager
    )
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Service for the Role Creation
    /// </summary>
    /// <param name="role"></param>
    /// <param name="ct"></param>
    /// <exception cref="Exception"></exception>
    public async Task CreateRole(RoleModel role, CancellationToken ct)
    {
        if (await _roleManager.RoleExistsAsync(role.Name))
        {
            throw new Exception("Role already exists");
        }

        await _roleManager.CreateAsync(new IdentityRole() { Name = role.Name });
    }
    
    /// <summary>
    /// Service for finding a Role by name
    /// </summary>
    /// <param name="roleName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IdentityRole> GetRole(string roleName, CancellationToken ct)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == default(IdentityRole))
        {
            throw new Exception("Role not found");
        }
        
        return role;
    }
}