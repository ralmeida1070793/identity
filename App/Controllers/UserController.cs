using System.Net.Http.Headers;
using App.Context;
using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

public class UserController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private const string rootEndpoint = "User";

    public UserController(
        ILogger<HomeController> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Endpoint for the retrieval of the Current User's Role. Calls the API with the current user's authentication token.
    /// </summary>
    /// <param name="currentRole"></param>
    /// <returns></returns>
    public IActionResult Index(RoleViewModel currentRole)
    {
        using (var client = new HttpClient())
        {
            // hack to test the unauthorized message from the API, if the user is not loggedin
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, String.Format("{0}/{1}/{2}", _configuration["ApiUrl"], rootEndpoint, String.IsNullOrEmpty(UserContext.GetInstance().UserName)?"Undefined":UserContext.GetInstance().UserName));
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UserContext.GetInstance().Token);
    
            var responseTask = client.SendAsync(httpRequestMessage);
            responseTask.Wait();

            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var currentUserRole = result.Content.ReadFromJsonAsync<string[]>().Result;
                currentRole.Name = currentUserRole.FirstOrDefault();
            }
            else
            {
                ViewBag.Message = result.ToString();
            }
        }
        
        return View(currentRole);
    }
}