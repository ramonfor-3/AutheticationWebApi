namespace AuthenticationWebApi.Entities;

public class RefreshTokenRequest
{
    public string TokenId { get; set; }
    public string RefreshToken { get; set; }
}