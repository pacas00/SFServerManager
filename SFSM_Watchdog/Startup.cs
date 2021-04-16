using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;

namespace SFSM_Watchdog
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
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SFSM_Watchdog", Version = "v1" });
            });
            
            services.AddTransient<Jobs.ProcessWatcher>();
            services.AddTransient<Jobs.AutoRestart>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SFSM_Watchdog v1"));
            }

            app.UseRouting();

            app.Use(async (context, next) =>
            {
                Console.WriteLine(context.Request.Method);
                Console.WriteLine(context.Request.Path.ToString()+context.Request.QueryString.ToString());
                Console.WriteLine(context.Request.ContentType + " " + context.Request.ContentLength);
                Console.WriteLine();
                await next.Invoke();
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //if (!Debug)
            {
                JobManager.Initialize(new ApplicationRegistry(app.ApplicationServices));
                JobManager.Start();
            }
        }
    }
}
