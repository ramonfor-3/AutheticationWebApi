using System.ComponentModel.DataAnnotations;

namespace AuthenticationWebApi.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    [Required]
    public string TokenId { get; set; }
    [Required]
    public string TokenHash { get; set; } 
    public int UserId { get; set; } 
    public User? User { get; set; }
    public DateTime ExpiryDate { get; set; } 
    public bool IsRevoked { get; set; } 
    public string UserAgent { get; set; }
    public string IpAddress { get; set; }  
    public DateTime CreatedAt { get; set; } 
}