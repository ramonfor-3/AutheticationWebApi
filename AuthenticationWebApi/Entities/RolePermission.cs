namespace AuthenticationWebApi.Entities;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public int PermissionId { get; set; }
    public Permissions Permission { get; set; }
    
}