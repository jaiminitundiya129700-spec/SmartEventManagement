using System.ComponentModel.DataAnnotations;
namespace SmartEventManagement.Models;
public class AppUser
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } = "User";
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; }
    public string ReturnUrl { get; set; }
}
public class RegisterVm
{
    [Required, StringLength(60)] public string Name { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters."), DataType(DataType.Password)] public string Password { get; set; }
    [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")] public string ConfirmPassword { get; set; }
}
