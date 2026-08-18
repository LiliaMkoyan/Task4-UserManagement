using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Task4.UserManagement.Models;

public class RegisterViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    [RegularExpression("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", 
    ErrorMessage = "Please enter a valid email address, e.g. example@example.com")]    
    public  string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}