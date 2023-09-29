using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using App.Context;
using Microsoft.AspNetCore.Mvc;
using App.Models;

namespace App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private const string rootEndpoint = "User";

    public HomeController(
        ILogger<HomeController> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Endpoint for Login Action. Calls the API, without requirements
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public IActionResult Index(LoginViewModel login)
    {
        if (ModelState.IsValid)
        {
            using (var client = new HttpClient())
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json")
                };
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));           
                
                var responseTask = client.PostAsync(String.Format("{0}/{1}/{2}", _configuration["ApiUrl"], rootEndpoint, "login"), httpRequestMessage.Content);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var token = result.Content.ReadFromJsonAsync<TokenViewModel>();
                    
                    UserContext.GetInstance().UserName = login.Username;
                    UserContext.GetInstance().Token = token.Result.Token;
                }
                else
                {
                    ViewBag.Message = "Username or password invalid";
                }
            }
        }
        
        return View(login);
    }

    /// <summary>
    /// Endpoint for Registration Action. Calls the API, without requirements
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public IActionResult Register(UserViewModel user)
    {
        if (ModelState.IsValid)
        {
            using (var client = new HttpClient())
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json")
                };
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));           
                
                var responseTask = client.PostAsync(String.Format("{0}/{1}/{2}", _configuration["ApiUrl"], rootEndpoint, "register"), httpRequestMessage.Content);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Registration Successful";
                }
                else
                {
                    ViewBag.Message = "Username or password invalid";
                }
            }
        }
        
        return View(user);
    }

    /// <summary>
    /// Endpoint for Logout Action. Updates the context's properties
    /// </summary>
    /// <returns></returns>
    public IActionResult Logout()
    {
        UserContext.GetInstance().UserName = "";
        UserContext.GetInstance().Token = "";

        return Redirect("/");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}