namespace AuthenticationWebApi.Entities;

public class UserCompanyLocation
{
    public string Id { get; set; }
    
    public User User { get; set; }
    public int UserId { get; set; }
    public Company Company { get; set; }
    public int CompanyId { get; set; }
    public Location Location { get; set; }
    public int LocationId { get; set; }
    public Role Role { get; set; } 
    public int RoleId { get; set; }      
    
    
}