using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;

namespace SFServerManager.Code.DataObjects
{
    public class ServerStatus
    {
        [JsonProperty("Game")]
        public Game Game { get; set; } = new Game();

        [JsonProperty("Plugins")]
        public List<Plugin> Plugins { get; set; } = new List<Plugin>();

        [JsonProperty("World")]
        public World World { get; set; } = new World();

        public string GetSMLVersion()
        {
            if (Plugins.FindAll(x => x.FriendlyName.Equals("Satisfactory Mod Loader")).Count != 0)
            {
                return Plugins.First(x => x.FriendlyName.Equals("Satisfactory Mod Loader")).VersionName;
            }

            return "Not Installed";
        }

        public string GetSFSMVersion()
        {
            if (Plugins.FindAll(x => x.FriendlyName.Equals("SFServerManager")).Count != 0)
            {
                return Plugins.First(x => x.FriendlyName.Equals("SFServerManager")).VersionName;
            }

            return "Not Installed";
        }

        public void Merge(ServerStatus serverStatus)
        {
            Game = serverStatus.Game;
            Plugins = serverStatus.Plugins;

            foreach (PropertyInfo property in typeof(World).GetProperties())
            {
                try
                {
                    if (property.GetValue(serverStatus.World) != null && IsNotEmpty(property.GetValue(serverStatus.World)))
                    {
                        property.SetValue(World, property.GetValue(serverStatus.World));
                    }
                    else if (property.GetValue(serverStatus.World) == null && property.PropertyType == typeof(long))
                    {
                        property.SetValue(World, 0);
                    }
                    else
                    {
                        if (property.GetValue(serverStatus.World) == null && property.PropertyType == typeof(string) && serverStatus.World.MapType.Contains("Menu"))
                        {
                            //Defaults
                            if (property.Name.Contains("SessionName"))
                            {
                                property.SetValue(World, "Main Menu");
                            }
                            else if (property.Name.Contains("SessionId"))
                            {
                                property.SetValue(World, "NONE");
                            }
                            else if (property.Name.Contains("ServerName"))
                            {
                                property.SetValue(World, "NONE");
                            }
                            else if (property.Name.Contains("SessionVisibility"))
                            {
                                property.SetValue(World, "NONE");
                            }
                            else if (property.Name.Contains("MapType"))
                            {
                                property.SetValue(World, serverStatus.World.MapType);
                            }
                            else if (property.Name.Contains("MapName"))
                            {
                                property.SetValue(World, serverStatus.World.MapName);
                            }
                            else
                            {
                                property.SetValue(World, "");
                            }


                        }
                        else if (property.GetValue(serverStatus.World) == null && property.PropertyType == typeof(string))
                        {
                            property.SetValue(World, "");
                        }
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private bool IsNotEmpty(object? getValue)
        {
            if (getValue == null) return false;
            if (getValue is string s && s.Length == 0) return false;
            if (getValue is long l && l == 0) return false;

            return true;
        }
    }

    
    public class Game
    {
        //GV_Main
        //GV_Experimental
        //GV_Other
        [JsonProperty("Branch")]
        public string Branch { get; set; }

        public string GetBranch()
        {
            switch (Branch)
            {
                case "GV_Main": return "Main";
                case "GV_Experimental": return "Experimental";
                case "GV_Other": return "Other (Early Access?)";
            }
            return "Unknown";
        }

        
        [JsonProperty("BranchDescriptor")]
        public string BranchDescriptor { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("Changelist")]
        public long Changelist { get; set; }

        public string GetEngine()
        {
            var engineSplit = Version.Split("-");
            return engineSplit[0];
        }
        public string GetSFBuild()
        {
            return Changelist.ToString();
        }
        public string GetSFVersion()
        {
            //var engineSplit = Engine.Split("+++");
            ////4.22.3-125236
            ////FactoryGame+rel-main-0.3.5

            //var tmp = Satisfactory;
            //tmp = tmp.TrimStart('+');
            ////FactoryGame+rel-main-0.3.5-CL-125236


            //var tmp2 = tmp.Replace(engineSplit[1], "");
            ////-CL-125236

            //tmp = tmp.Replace(tmp2, "");
            ////FactoryGame+rel-main-0.3.5

            //tmp = tmp.Substring(tmp.LastIndexOf("-") + 1);

            return "Unknown";
        }
    }

    public class Plugin
    {
        [JsonProperty("FriendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("VersionName")]
        public string VersionName { get; set; }

        [JsonProperty("SemVersion")]
        public string SemVersion { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedByURL")]
        public string CreatedByUrl { get; set; }
    }

    public class World
    {
        [JsonProperty("MapName")]
        public string MapName { get; set; }

        [JsonProperty("MapType")]
        public string MapType { get; set; }

        [JsonProperty("PlayerCount")]
        public long PlayerCount { get; set; }

        [JsonProperty("ServerName")]
        public string ServerName { get; set; }

        [JsonProperty("MaxPlayers")]
        public long MaxPlayers { get; set; }

        [JsonProperty("SessionJoinable")]
        public string SessionJoinable { get; set; }

        [JsonProperty("SessionVisibility")]
        public string SessionVisibility { get; set; }

        [JsonProperty("SessionName")]
        public string SessionName { get; set; }

        [JsonProperty("SessionID")]
        public string SessionId { get; set; }

        [JsonProperty("NATType")]
        public string NatType { get; set; }

        [JsonProperty("BuildVersion")]
        public string BuildVersion { get; set; }

        [JsonProperty("PlayDuration")]
        public long PlayDuration { get; set; }

        [JsonProperty("ConnectedPlayers")]
        public long ConnectedPlayers { get; set; }
    }
}

