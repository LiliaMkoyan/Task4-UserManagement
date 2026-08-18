using System.ComponentModel.DataAnnotations;

namespace Task4.UserManagement.Models;
public enum AccountStatus
{
    Unverified,
    Active,
    Blocked
}

public class UsersModel
{
    public int Id  { get; set; }
    public string Name { get; set; }
    
    [EmailAddress]
    public string Email  { get; set; }
    public string PasswordHash  { get; set; }
    public DateTime LastActive  { get; set; }
    public DateTime RegisteredTime  { get; set; }
    public AccountStatus Status { get; set; }
}