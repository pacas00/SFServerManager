using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFServerManager.Code.DataObjects;
using Syncfusion.Blazor.Grids;

namespace SFServerManager.Code.Instanced.Services
{
    public enum EDataType
    {
        String,
        Boolean,
        Integer,
        Double
    }
    public class Setting
    {
        public Setting()
        {
            DataType = EDataType.String;
        }

        public Setting(string Key, string Value)
        {
            this.Key = Key;
            this.Value = Value;
            DataType = EDataType.String;
        }

        public Setting(string Key, bool Value)
        {
            this.Key = Key;
            this.Value = Convert.ToString(Value);
            DataType = EDataType.Boolean;
        }

        public Setting(string Key, int Value) 
        {
            this.Key = Key;
            this.Value = Convert.ToString(Value);
            DataType = EDataType.Integer;
        }

        public Setting(string Key, double Value) 
        {
            this.Key = Key;
            this.Value = Convert.ToString(Value);
            DataType = EDataType.Double;
        }

        [BsonId]
        public ObjectId Id { get; set; }
        
        public string Key { get; set; }
        public string Value { get; set; }
        
        public EDataType DataType { get; set; }

        public bool AsBool()
        {
            return Convert.ToBoolean(Value);
        }
        public int AsInt()
        {
            return Convert.ToInt32(Value);
        }
        public double AsDouble()
        {
            return Convert.ToDouble(Value);
        }

        public EDataType DetermineType()
        {
            int numi;
            double numd;
            bool numb;

            if (bool.TryParse(Value, out numb))
            {
                return EDataType.Boolean;
            }
            
            if (int.TryParse(Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out numi))
            {
                return EDataType.Integer;
            }
            
            if (double.TryParse(Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out numd))
            {
                return EDataType.Double;
            }
            
            return EDataType.String;
        }
    }


    //SINGLETON
    //THERE IS ONLY ONE
    public class FirstTimeSetupService : BackgroundService
    {
        private readonly DatabaseService db;
        private readonly IServiceProvider _services;
        private readonly ILogger<FirstTimeSetupService> _logger;

        public FirstTimeSetupService(IServiceProvider services, ILogger<FirstTimeSetupService> logger, DatabaseService databaseService)
        {
            db = databaseService;
            _services = services;
            _logger = logger;
        }

        /// <inheritdoc />
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!db.HasCollection("Settings"))
            {
                string user = "Admin" + NumberGenerators.GetRandomHexNumber(2);
                string pass = NumberGenerators.GetRandomHexNumber(16);
                
                var col = db.GetCollection<Setting>("Settings");
                col.EnsureIndex(x => x.Key);
                col.Insert(new Setting("System.AdminUsername", user));
                col.Insert(new Setting("System.AdminPass", pass));
                col.Insert(new Setting("System.AdminPort", 8989));

                
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("First Time Setup Complete!");
                Console.WriteLine("");
                Console.WriteLine("Admin Username: " + user);
                Console.WriteLine("Admin Password: " + pass);
                Console.WriteLine("");
                Console.WriteLine("These will not be shown again.");
                Console.WriteLine("-----------------------------------------------------");
                
                
                //Ensure test Server is added.
                db.Insert<ServerEntry>("Servers", new ServerEntry()
                {
                    Uuid = Guid.Parse("7acde728-6d3a-437a-aefd-ef6a20c58461"),
                    Apikey = "C366020887DF753F10B335D9E0A031D8",
                    Hostname = "localhost",
                    SFSM_Port = 8033,
                    LastStatus = new ServerStatus()
                });
                
                
            }


            return Task.CompletedTask;
        }
    }
}
