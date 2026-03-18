using System.ComponentModel.DataAnnotations;

namespace MvcIngredient.Models;

public class User
{
    public User()
    {

    }

    public User(string userName)
    {
        UserName = userName;
        UserPassword = "";
    }
    
    public User(string userName, string userPassword)
    {
        UserName = userName;
        UserPassword = userPassword;
    }

    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string? UserName { get; set; }

    [StringLength(255)]
    public string? UserPassword { get; set; }
}