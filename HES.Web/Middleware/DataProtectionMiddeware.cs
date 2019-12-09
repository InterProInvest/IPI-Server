using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HES.Web.Middleware
{
    public class DataProtectionMiddeware
    {
        private readonly RequestDelegate _next;
        private readonly IDataProtectionService _dataProtectionService;

        public DataProtectionMiddeware(RequestDelegate next, IDataProtectionService dataProtectionService)
        {
            _next = next;
            _dataProtectionService = dataProtectionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var status = _dataProtectionService.Status();

                switch (status)
                {
                    case ProtectionStatus.Off:
                        if (context.Request.Path != "/Settings/DataProtection/SetPassword")
                        {
                            context.Response.Redirect($"/Settings/DataProtection/SetPassword", true);
                        }
                        break;
                    case ProtectionStatus.Activate:
                        if (context.Request.Path != "/Settings/DataProtection/Activate")
                        {
                            context.Response.Redirect($"/Settings/DataProtection/Activate?returnUrl={context.Request.Path}", true);
                        }
                        break;
                }
            }

            await _next.Invoke(context);
        }
    }
}