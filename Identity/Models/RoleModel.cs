using System.ComponentModel.DataAnnotations;

namespace Identity.Models;

public class RoleModel
{
    [Required(ErrorMessage = "Name is required")]
    public string? Name { get; set; }
}