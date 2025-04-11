using AuthenticationWebApi.Filters;

namespace AuthenticationWebApi.ServiceInterface;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(HasPermissionFilter  filter);
    Task<bool> HasPermissionAsync(HasPermissionFilter filter);
}