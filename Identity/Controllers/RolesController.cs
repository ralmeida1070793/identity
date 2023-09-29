using Identity.Models;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Identity.Controllers;

/// <summary>
/// Roles Controller - It is only available for calls from an Administrator's Token
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrator")]
public class RolesController : ControllerBase
{
    protected IRoleServices _roleServices;

    /// <summary>
    /// Conntructor with the roleService dependency injected
    /// </summary>
    /// <param name="roleServices"></param>
    public RolesController(
        IRoleServices roleServices
    )
    {
        _roleServices = roleServices;
    }

    /// <summary>
    /// Endpoint for the registration of a new Role
    /// </summary>
    /// <param name="role"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] RoleModel role, CancellationToken ct)
    {
        try
        {
            await _roleServices.CreateRole(role, ct);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
}