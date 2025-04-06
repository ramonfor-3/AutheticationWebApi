namespace AuthenticationWebApi.Entities;

public class User
{
    public int Id { get; set; } 
    public string Username { get; set; } 
    public string PasswordHash { get; set; } 
    public string Email { get; set; } 
    public bool IsActive { get; set; } 
    public DateTime CreatedAt { get; set; } 
    public DateTime? LastLogin { get; set; } 
}