namespace AuthenticationWebApi.Entities;

public class PasswordChangeRequest
{
    public string OldPassword { get; set; } 
    public string NewPassword { get; set; } 
}