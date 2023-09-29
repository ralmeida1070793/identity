using System.IdentityModel.Tokens.Jwt;
using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    protected IUserServices _userServices;
    protected IRoleServices _roleServices;

    /// <summary>
    /// /// Conntructor with the userService and the roleService dependencies injected
    /// </summary>
    /// <param name="userServices"></param>
    /// <param name="roleServices"></param>
    public UserController(
        IUserServices userServices,
        IRoleServices roleServices
    )
    {
        _userServices = userServices;
        _roleServices = roleServices;
    }

    /// <summary>
    /// Endpoint for the authentication of a user
    /// </summary>
    /// <param name="model"></param>
    /// <param name="ct"></param>
    /// <returns>Authentication Token</returns>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken ct)
    {
        try
        {
            var token = await _userServices.Login(model.Username, model.Password, ct);
            return Ok(
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo    
                }
            );
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }
    
    /// <summary>
    /// Endpoint for the registration of a new User
    /// </summary>
    /// <param name="model"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, CancellationToken ct)
    {
        try
        {
            var userExists = await _userServices.GetUserByName(model.Username, ct);
            if (userExists != default(IdentityUser))
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                "User already exists!"
                );

            await _userServices.CreateUser(
                new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                },
                model.Password,
                await _roleServices.GetRole(model.Role, ct),
                ct
            );

            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    /// <summary>
    /// Endpoint for the retrieval of a user's role. Only available for authenticated users.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="ct"></param>
    /// <returns>string[]</returns>
    [Authorize]
    [HttpGet, Route("{userName}")]
    public async Task<IActionResult> GetUserRoles([FromRoute] string userName, CancellationToken ct)
    {
        var user = await _userServices.GetUserByName(userName, ct);
        if (user == default(IdentityUser))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "User not found");
        }

        var roles = await _userServices.GetUserRoles(user, ct);
        return Ok(roles);
    }
}