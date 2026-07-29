using MediTrack.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MediTrack.Mvc.Filters;

public class AuditAccessDeniedFilter : IAsyncAuthorizationFilter
{
    private readonly IAuditLogService _auditLogService;

    public AuditAccessDeniedFilter(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;

        if (httpContext.User.Identity?.IsAuthenticated != true)
            return;

        if (context.Result != null)
        {
            await _auditLogService.LogAsync("AccessDenied", "Route", httpContext.Request.Path, "Failed",
                $"Method={httpContext.Request.Method}, Policy={context.Result}");
        }
    }
}
