namespace AuthenticationWebApi.Entities;

public class Permissions
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }

}