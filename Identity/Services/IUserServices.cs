using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace Identity.Services;

public interface IUserServices
{
    Task CreateUser(IdentityUser user, string password, IdentityRole role, CancellationToken ct);
    Task UpdateUser(IdentityUser user, IdentityRole role, CancellationToken ct);
    Task DeleteUser(string userId, CancellationToken ct);
    Task<IdentityUser> GetUserById(string userId, CancellationToken ct);
    Task<IdentityUser> GetUserByName(string userName, CancellationToken ct);
    Task<ICollection<string>> GetUserRoles(IdentityUser user, CancellationToken ct);
    Task<ICollection<IdentityUser>> ListUsersInRole(string roleName, CancellationToken ct);
    Task<JwtSecurityToken> Login(string userName, string password, CancellationToken ct);
}