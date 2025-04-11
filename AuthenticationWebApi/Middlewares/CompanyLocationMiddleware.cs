namespace AuthenticationWebApi.Middlewares;

public class CompanyLocationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var userClaims = context.User.Claims;
        var requestedEmpresaId = context.Request.Headers["CompanyId"];
        var requestedLocalidadId = context.Request.Headers["LocationId"];

        if (!userClaims.Any(c => c.Type == "company_id" && c.Value == requestedEmpresaId)
            || !userClaims.Any(c => c.Type == "location_id" && c.Value == requestedLocalidadId))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Acceso denegado a esta empresa o localidad.");
            return;
        }

        await next(context);
    }
}