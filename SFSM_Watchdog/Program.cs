using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using FluentScheduler;

namespace SFSM_Watchdog
{
    public class Program
    {
        public static DateTime StartTime          = DateTime.Now;
        public static DateTime NextAutoRestart    = DateTime.Now;
        public static DateTime NextProgramRestart = DateTime.Now.AddDays(1);

        private static CancellationTokenSource cancelTokenSource = new System.Threading.CancellationTokenSource();

        public static void Restart()
        {
            cancelTokenSource.Cancel();
        }

        public static void Main(string[] args)
        {
            if (Configuration.LoadSettings())
            {
                IHost server = CreateHostBuilder(args).Build();
                server.RunAsync(cancelTokenSource.Token).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("Please Check Configuration.ini and try again");
                Console.ReadLine();
            }
        }


        

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            
        }

        private static void JobManager_JobException(JobExceptionInfo obj)
        {
            Configuration.WriteLine(obj.Name);
            Configuration.WriteLine(obj.Exception);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://" + Configuration.WebListenIP + ":" + Configuration.WebPort);
                });
    }
}
