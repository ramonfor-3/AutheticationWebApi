namespace AuthenticationWebApi.Entities;

public class RefreshToken
{
    public string Token { get; set; } 
    public int UserId { get; set; } 
    public DateTime ExpiryDate { get; set; } 
    public bool IsRevoked { get; set; } 
}