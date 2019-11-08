using HES.Core.Entities;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Services;
using HES.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;

namespace HES.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var server = configuration["MYSQL_SRV"];
            var port = configuration["MYSQL_PORT"];
            var db = configuration["MYSQL_DB"];
            var uid = configuration["MYSQL_UID"];
            var pwd = configuration["MYSQL_PWD"];

            if (server != null && port != null && db != null && uid != null && pwd != null)
            {
                configuration["ConnectionStrings:DefaultConnection"] = $"server={server};port={port};database={db};uid={uid};pwd={pwd}";
            }

            var email_host = configuration["EMAIL_HOST"];
            var email_port = configuration["EMAIL_PORT"];
            var email_ssl = configuration["EMAIL_SSL"];
            var email_user = configuration["EMAIL_USER"];
            var email_pwd = configuration["EMAIL_PWD"];

            if (email_host != null && email_port != null && email_ssl != null && email_user != null && email_pwd != null)
            {
                configuration["EmailSender:Host"] = email_host;
                configuration["EmailSender:Port"] = email_port;
                configuration["EmailSender:EnableSSL"] = email_ssl;
                configuration["EmailSender:UserName"] = email_user;
                configuration["EmailSender:Password"] = email_pwd;
            }

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Data protection keys
            services.AddDataProtection()
                .SetApplicationName("HES")
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "dataprotection")));

            services.AddSignalR();

            // Add Services
            services.AddScoped(typeof(IAsyncRepository<>), typeof(Repository<>));

            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IEmployeeService, EmployeeService>();

            services.AddScoped<IWorkstationSessionService, WorkstationSessionService>();

            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDeviceTaskService, DeviceTaskService>();
            services.AddScoped<IDeviceAccountService, DeviceAccountService>();
            services.AddScoped<IDeviceAccessProfilesService, DeviceAccessProfilesService>();

            services.AddScoped<IWorkstationService, WorkstationService>();
            services.AddScoped<IProximityDeviceService, ProximityDeviceService>();
            services.AddScoped<IWorkstationEventService, WorkstationEventService>();

            services.AddScoped<ISharedAccountService, SharedAccountService>();
            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<IApplicationUserService, ApplicationUserService>();
            services.AddScoped<IOrgStructureService, OrgStructureService>();
            services.AddScoped<ISamlIdentityProviderService, SamlIdentityProviderService>();
            services.AddScoped<IAppService, AppService>();
            services.AddScoped<ILogViewerService, LogViewerService>();
            services.AddTransient<IAesCryptographyService, AesCryptographyService>();

            services.AddSingleton<IDataProtectionService, DataProtectionService>(s =>
            {
                var scope = s.CreateScope();
                var dataProtectionRepository = scope.ServiceProvider.GetService<IAsyncRepository<DataProtection>>();
                var deviceRepository = scope.ServiceProvider.GetService<IAsyncRepository<Device>>();
                var deviceTaskRepository = scope.ServiceProvider.GetService<IAsyncRepository<DeviceTask>>();
                var sharedAccountRepository = scope.ServiceProvider.GetService<IAsyncRepository<SharedAccount>>();
                var dataProtectionProvider = scope.ServiceProvider.GetService<IDataProtectionProvider>();
                var logger = scope.ServiceProvider.GetService<ILogger<DataProtectionService>>();
                return new DataProtectionService(dataProtectionRepository,
                                                 deviceRepository,
                                                 deviceTaskRepository,
                                                 sharedAccountRepository,
                                                 dataProtectionProvider,
                                                 logger);
            });

            services.AddScoped<IRemoteWorkstationConnectionsService, RemoteWorkstationConnectionsService>();
            services.AddScoped<IRemoteDeviceConnectionsService, RemoteDeviceConnectionsService>();
            services.AddScoped<IRemoteTaskService, RemoteTaskService>();
            
            services.AddSingleton<IEmailSenderService, EmailSenderService>(i =>
                 new EmailSenderService(
                     Configuration["EmailSender:Host"],
                     Configuration.GetValue<int>("EmailSender:Port"),
                     Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                     Configuration["EmailSender:UserName"],
                     Configuration["EmailSender:Password"]));

            // Cookie
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Dismiss strong password
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            });

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Auth policy
            services.AddAuthorization(config =>
            {
                config.AddPolicy("RequireAdministratorRole",
                    policy => policy.RequireRole("Administrator"));
                config.AddPolicy("RequireUserRole",
                    policy => policy.RequireRole("User"));
            });

            // Mvc
            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage", "RequireAdministratorRole");
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/External");

                    options.Conventions.AddPageRoute("/Dashboard/Index", "");
                    options.Conventions.AuthorizeFolder("/Dashboard", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Employees", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Workstations", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/SharedAccounts", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Templates", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Devices", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Audit", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Settings", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Logs", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Notifications", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Develop", "RequireAdministratorRole");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePages();

            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("en-GB"),
                new CultureInfo("en"),

                new CultureInfo("fr-FR"),
                new CultureInfo("fr"),

                new CultureInfo("it-IT"),
                new CultureInfo("it"),

                new CultureInfo("uk-UA"),
                new CultureInfo("uk"),

                new CultureInfo("ru-RU"),
                new CultureInfo("ru-UA"),
                new CultureInfo("ru"),

                new CultureInfo("de-DE"),
                new CultureInfo("de")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSignalR(routes =>
            {
                routes.MapHub<DeviceHub>("/deviceHub");
                routes.MapHub<AppHub>("/appHub");
                routes.MapHub<EmployeeDetailsHub>("/employeeDetailsHub");
            });
            app.UseMvc();
            app.UseCookiePolicy();

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // Apply migration
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
                // Db seed if first run
                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                new ApplicationDbSeed(context, userManager, roleManager).Initialize();
                // Get status of data protection
                var dataProtectionService = scope.ServiceProvider.GetService<IDataProtectionService>();
                dataProtectionService.Status();
            }
        }
    }
}
