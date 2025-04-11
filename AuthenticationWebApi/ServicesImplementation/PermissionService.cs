using AuthenticationWebApi.Filters;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebApi.ServicesImplementation;

public class PermissionService(AuthenticationContext authContext) : IPermissionService
{
    public async Task<List<string>> GetUserPermissionsAsync(HasPermissionFilter filter)
    {
        var userRoleId = await authContext.UserCompanyLocations
            .Where(x => x.UserId == filter.UserId && x.CompanyId == filter.CompanyId && x.LocationId == filter.LocationId)
            .Select(x => x.RoleId)
            .FirstOrDefaultAsync();

        if (userRoleId == 0) return [];
        
        var permission = await authContext.RolePermissions
            .Where(rp => rp.RoleId == userRoleId)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();

        return permission;
    }
    public async Task<bool> HasPermissionAsync(HasPermissionFilter filter)
    {
        var permission = await GetUserPermissionsAsync(filter);
        return permission.Contains(filter.PermissionCode, StringComparer.OrdinalIgnoreCase);
    }
}