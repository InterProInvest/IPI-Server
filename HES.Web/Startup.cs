using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.HostedServices;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Services;
using HES.Infrastructure;
using HES.Web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace HES.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            #region Environment variables

            var server = configuration["MYSQL_SRV"];
            var port = configuration["MYSQL_PORT"];
            var db = configuration["MYSQL_DB"];
            var uid = configuration["MYSQL_UID"];
            var pwd = configuration["MYSQL_PWD"];

            if (server != null && port != null && db != null && uid != null && pwd != null)
                configuration["ConnectionStrings:DefaultConnection"] = $"server={server};port={port};database={db};uid={uid};pwd={pwd}";

            var dataprotectoin_pwd = configuration["DATAPROTECTION_PWD"];

            if (dataprotectoin_pwd != null)
                configuration["DataProtection:Password"] = dataprotectoin_pwd;

            #endregion

            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Services
            services.AddScoped(typeof(IAsyncRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IMainTableService<,>), typeof(MainTableService<,>));
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IHardwareVaultService, HardwareVaultService>();
            services.AddScoped<IHardwareVaultTaskService, HardwareVaultTaskService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IWorkstationService, WorkstationService>();
            services.AddScoped<IWorkstationAuditService, WorkstationAuditService>();
            services.AddScoped<ISharedAccountService, SharedAccountService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IApplicationUserService, ApplicationUserService>();
            services.AddScoped<IOrgStructureService, OrgStructureService>();
            services.AddScoped<ILogsViewerService, LogsViewerService>();
            services.AddScoped<IRemoteWorkstationConnectionsService, RemoteWorkstationConnectionsService>();
            services.AddScoped<IRemoteDeviceConnectionsService, RemoteDeviceConnectionsService>();
            services.AddScoped<IRemoteTaskService, RemoteTaskService>();
            services.AddScoped<IEmailSenderService, EmailSenderService>();
            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<IAppSettingsService, AppSettingsService>();
            services.AddScoped<IToastService, ToastService>();
            services.AddScoped<IModalDialogService, ModalDialogService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<ILdapService, LdapService>();
            services.AddScoped<ISoftwareVaultService, SoftwareVaultService>();

            services.AddSingleton<IBreadcrumbsService, BreadcrumbsService>();
            services.AddSingleton<IDataProtectionService, DataProtectionService>();

            services.AddHostedService<RemoveLogsHostedService>();
            services.AddHostedService<LicenseHostedService>();

            services.AddHttpClient().RemoveAll<IHttpMessageHandlerBuilderFilter>();
            services.AddSignalR();

            // Cookie
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.Cookie = new CookieBuilder
                {
                    IsEssential = true // required for auth to work without explicit user consent; adjust to suit your privacy policy
                };
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
                .AddDefaultUI()
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

            // Override OnRedirectToLogin via API
            services.ConfigureApplicationCookie(config =>
            {
                config.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.CompletedTask;
                    }
                };
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
                    options.Conventions.AuthorizeFolder("/Groups", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Workstations", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/SharedAccounts", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Templates", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/HardwareVaults", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/SoftwareVaults", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Audit", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Settings", "RequireAdministratorRole");
                    options.Conventions.AuthorizeFolder("/Logs", "RequireAdministratorRole");
                })
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddControllers();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddServerSideBlazor();

            // Localization Options
            services.Configure<RequestLocalizationOptions>(options =>
            {
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

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HES API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<DataProtectionMiddeware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HES API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DeviceHub>("/deviceHub");
                endpoints.MapHub<AppHub>("/appHub");
                endpoints.MapHub<EmployeesHub>("/employeesHub");
                endpoints.MapHub<EmployeeDetailsHub>("/employeeDetailsHub");
                endpoints.MapHub<SharedAccountsHub>("/sharedAccountsHub");
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
            });

            app.UseCookiePolicy();

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
            logger.LogInformation($"Server started {ServerConstants.Version}");
        }
    }
}
