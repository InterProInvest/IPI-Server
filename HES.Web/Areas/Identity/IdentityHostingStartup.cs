using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(HES.Web.Areas.Identity.IdentityHostingStartup))]
namespace HES.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => 
            {

            });
        }
    }
}