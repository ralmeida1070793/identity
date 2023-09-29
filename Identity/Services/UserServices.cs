using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Services;

public class UserServices : IUserServices
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor with the UserManager and the AppSettings Configurations dependencies injected
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="configuration"></param>
    public UserServices(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Service for the registration of a new User
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="role"></param>
    /// <param name="ct"></param>
    /// <exception cref="Exception"></exception>
    public async Task CreateUser(IdentityUser user, string password, IdentityRole role, CancellationToken ct)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception("User creation failed! Please check user details and try again.");
            
        await _userManager.AddToRoleAsync(user, role.Name);
    }
    
    /// <summary>
    /// Service for the data update of a User
    /// </summary>
    /// <param name="user"></param>
    /// <param name="role"></param>
    /// <param name="ct"></param>
    /// <exception cref="Exception"></exception>
    public async Task UpdateUser(IdentityUser user, IdentityRole role, CancellationToken ct)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception("User creation failed! Please check user details and try again.");

        var presisted = await _userManager.GetRolesAsync(user);
        if (presisted.Any())
        {
            await _userManager.RemoveFromRoleAsync(user, presisted.FirstOrDefault());
        }
        
        await _userManager.AddToRoleAsync(user, role.Name);
    }

    /// <summary>
    /// Service for the deletion of a User
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <exception cref="Exception"></exception>
    public async Task DeleteUser(string userId, CancellationToken ct)
    {
        IdentityUser user = await _userManager.FindByIdAsync(userId);
        if (user == default(IdentityUser))
        {
            throw new Exception("User not found.");
        }

        await _userManager.DeleteAsync(user);
    }
    
    /// <summary>
    /// Service for the retrieval of a user's data by Username
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IdentityUser> GetUserByName(string userName, CancellationToken ct)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    /// <summary>
    /// Service for the retrieval of a user's data by Id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IdentityUser> GetUserById(string userId, CancellationToken ct)
    {
        IdentityUser user = await _userManager.FindByIdAsync(userId);
        if (user == default(IdentityUser))
        {
            throw new Exception("User not found.");
        }

        return user;
    }
    
    /// <summary>
    /// Service for the retrieval of the Assigned Roles of a provided user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ICollection<string>> GetUserRoles(IdentityUser user, CancellationToken ct)
    {
        var response = await _userManager.GetRolesAsync(user);
        return response.ToList();
    }

    /// <summary>
    /// Service to List the Users within a provided Role
    /// </summary>
    /// <param name="roleName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ICollection<IdentityUser>> ListUsersInRole(string roleName, CancellationToken ct)
    {
        var response = await _userManager.GetUsersInRoleAsync(roleName);
        return response.ToList();
    }

    /// <summary>
    /// Service for the Authentication of a User
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<JwtSecurityToken> Login(string userName, string password, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == default(IdentityUser))
        {
            throw new Exception("User not found");
        }

        if (!(await _userManager.CheckPasswordAsync(user, password)))
        {
            throw new Exception("Invalid username or password");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }
        
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        return new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
    }
}