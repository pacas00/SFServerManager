using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFServerManager.Code.Instanced.Services;

namespace SFServerManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            int port = 8989;

            //Try and load the port from settings
            using (var db = new LiteDatabase("DataStorage.db"))
            {
                if (db.CollectionExists("Settings"))
                {
                    if (db.GetCollection<Setting>("Settings").Exists(x => x.Key == "System.AdminPort"))
                    {
                        port = db.GetCollection<Setting>("Settings").Find(x => x.Key == "System.AdminPort").First().AsInt();
                    }
                }
            }
            
            port = GetAvailablePort(port);
            string ListenURL = $"http://+:{port}";
            
            Console.WriteLine("Starting with URL " + ListenURL);

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseUrls(ListenURL);
                     webBuilder.UseStartup<Startup>();
                 });
        }

        private static int GetAvailablePort(int portIn)
        {
            bool isAvailable = true;
            int port = portIn;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port==port)
                {
                    isAvailable = false;
                    break;
                }
            }

            if (!isAvailable)
            {
                for (int i = 8080; i < 8999; i++)
                {
                    foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                    {
                        if (tcpi.LocalEndPoint.Port==port)
                        {
                            continue;
                        }
                    }

                    //We should only get here IF the port is not in use.
                    port = i;
                    break;
                }
            }
            
            return port;
        }
    }
}
