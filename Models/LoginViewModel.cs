using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Task4.UserManagement.Models;

public class LoginViewModel
{
    [Required]
    [RegularExpression("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", 
    ErrorMessage = "Please enter a valid email address, e.g. example@example.com")]
    public  string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}