using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Widely.API.Infrastructure.NLog
{

    /// <summary>
    /// Render the username request for ASP.NET Core
    /// </summary>
    /// <example>
    /// <code lang="NLog Layout Renderer">
    /// ${aspnet-request-ip}
    /// </code>
    /// </example>
    [LayoutRenderer("aspnet-request-username")]
    public class UserRequestLayoutRenderer : AspNetLayoutRendererBase
    {
        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }
            builder.Append(httpContext.User.FindFirstValue(ClaimTypes.Name));
        }
    }
}
