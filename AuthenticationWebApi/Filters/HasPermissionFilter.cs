namespace AuthenticationWebApi.Filters;

public class HasPermissionFilter
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public int LocationId { get; set; }
    public string? PermissionCode { get; set; }
}