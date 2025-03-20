using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace smart_complaint_management_system.Areas.Admin.Controllers
{
    public class RestrictIPAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedIPs = new string[]
        {
           /*"x.x.x.x"*//// Example: your ip
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!_allowedIPs.Contains(remoteIp))
            {
                context.Result = new ContentResult
                {
                    Content = "Access Denied",
                    StatusCode = 403
                };
            }
            base.OnActionExecuting(context);
        }
    }
}
