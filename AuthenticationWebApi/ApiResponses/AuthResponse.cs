namespace AuthenticationWebApi.Entities;

public class AuthResponse
{
    public string AccessToken { get; set; } 
    public string RefreshToken { get; set; } 
    public string TokenId { get; set; }
    public DateTime Expiration { get; set; } 
}