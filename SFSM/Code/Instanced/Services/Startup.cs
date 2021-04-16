using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SFServerManager.Code.Instanced.Services
{
   
    //SINGLETON
    //THERE IS ONLY ONE
    public class StartupService : BackgroundService
    {
        private readonly DatabaseService db;
        private readonly IServiceProvider _services;
        private readonly ILogger<StartupService> _logger;

        public StartupService(IServiceProvider services, ILogger<StartupService> logger, DatabaseService databaseService)
        {
            db = databaseService;
            _services = services;
            _logger = logger;
        }

        /// <inheritdoc />
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {


            return Task.CompletedTask;
        }
    }
}
