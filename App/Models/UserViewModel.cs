using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class UserViewModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
    public string Role { get; set; }
}