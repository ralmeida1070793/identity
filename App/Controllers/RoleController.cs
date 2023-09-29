using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using App.Context;
using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

public class RoleController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private const string rootEndpoint = "Roles";

    public RoleController(
        ILogger<HomeController> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
    }

     
    /// <summary>
    /// Calls the API for the insertion of a new Role. This feature is exclusive for Administrator, thus it requires a token from an Administrator account
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public IActionResult Index(RoleViewModel role)
    {
        if (ModelState.IsValid)
        {
            /* Request works perfectly in Postman, but returns 401 while calling the endpoint through the HttpClient. The token and the URI are being correctly fed into the request, as well as the Accept Header */
            using (var client = new HttpClient())
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Content = new StringContent(JsonSerializer.Serialize(role), Encoding.UTF8, "application/json")
                };
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UserContext.GetInstance().Token);

                var responseTask = client.PostAsync(
                    String.Format("{0}/{1}/", _configuration["ApiUrl"], rootEndpoint),
                    httpRequestMessage.Content);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Registration Successful";
                }
                else
                {
                    ViewBag.Message = result.ToString();
                }
            }
        }

        return View(role);
    }
}