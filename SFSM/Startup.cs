using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using SFServerManager.Code.Instanced.Services;
using SFServerManager.Code.JobSystem;
using Syncfusion.Blazor;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace SFServerManager
{
    public class Startup
    {
        #if DEBUG
        public static bool Debug = true;
        #else
        public static bool Debug = false;
        #endif

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAuthentication(
                    CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie();
            
            services.AddRazorPages().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = false;
            });
            services.AddServerSideBlazor();
            services.AddSyncfusionBlazor();
            services.AddSingleton<DatabaseService>();
            services.AddHostedService<FirstTimeSetupService>();
            
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();

            // Register jobs as transient so that a fresh job instance is created
            // for each job execution.
            services.AddTransient<ServerStatusChecker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseRouting();
            
            app.Use(async (context, next) =>
            {
                Console.WriteLine(context.Request.Method);
                Console.WriteLine(context.Request.Path.ToString()+context.Request.QueryString.ToString());
                Console.WriteLine(context.Request.ContentType + " " + context.Request.ContentLength);
                Console.WriteLine();
                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                
                endpoints.MapFallbackToPage("/_Host");
            });

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjg5NjM3QDMxMzgyZTMyMmUzMEVMTGVnR1hkWFJGR0E2dGthR2c0bkZiL21uMThrOXdoTUpRTXIyZDFMdlE9;Mjg5NjM4QDMxMzgyZTMyMmUzMGNBb0hmNTVsUHBpWW1MS1Z5OXgrZXdEelhMTWwraUtCcU1uYmppVE83dXM9;Mjg5NjM5QDMxMzgyZTMyMmUzMEhJZ0VEQm9iMjBLWkRhMXBjNG8wU2txU1JxVkhWd1dxTEREazY1amMwZ1k9");

            //if (!Debug)
            {
                JobManager.Initialize(new ApplicationRegistry(app.ApplicationServices));
                JobManager.Start();
            }
        }
    }
}
