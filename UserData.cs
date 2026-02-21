namespace LocalLink.Models;

public class UserData
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string ProfileImagePath { get; set; } = "default_avatar.png";
}
