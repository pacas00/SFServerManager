using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using SFServerManager.Code.DataObjects;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace SFServerManager.Code
{
    public partial class CommandResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

    }

    public partial class Data
    {
        [JsonProperty("Request")]
        public CommandRequest Request { get; set; }
    }

    public class CommandRequest
    {
        [JsonProperty("accesssecret")]
        public string Accesssecret { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("commandjsondata")]
        public string Commandjsondata { get; set; }
    }
    
    public class StatusResponse
    {
        [JsonProperty("data")]
        public ServerStatus Data { get; set; } = new ServerStatus();

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }
    }

    public class ServerEntry
    {
        public ServerEntry()
        {
        }

        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }

        [JsonIgnore] 
        public string Nickname { get; set; } = "Satisfactory Server";

        
        public Guid Uuid { get; set; }
        public string Apikey { get; set; }
        public string Hostname { get; set; }
        public int SFSM_Port { get; set; } = -1;
        public int Watchdog_Port { get; set; } = -1;
        public ServerStatus LastStatus { get; set; }

        public string buildURL(string endpoint)
        {
            if (Watchdog_Port != -1)
            {
                return $"http://{Hostname}:{Watchdog_Port}/{endpoint}";
            }
            if (SFSM_Port != -1)
            {
                return $"http://{Hostname}:{SFSM_Port}/{endpoint}";
            }

            return "";
        }
    }

    

    ////GameStats

    //public partial class GameStats
    //{
        
    //    [BsonId]
    //    [JsonIgnore]
    //    public ObjectId Id { get; set; }
        
    //    [JsonIgnore]
    //    public Guid Uuid { get; set; }

        
    //    [JsonIgnore]
    //    public DateTimeOffset LastUpdated { get; set; }


    //    [JsonProperty("Game")]
    //    public Game Game { get; set; }

    //    [JsonProperty("Mods")]
    //    public List<Mod> Mods { get; set; }

    //    [JsonProperty("World")]
    //    public World World { get; set; }

    //    public static GameStats CreateDummy()
    //    {
    //        GameStats stats = new GameStats();
            
    //        stats.Game = new Game();
    //        stats.World = new World();
    //        stats.Mods = new List<Mod>();
            
    //        return stats;
    //    }
    //}

    //public partial class Game
    //{
    //    [JsonProperty("Engine")]
    //    public string Engine { get; set; }

    //    [JsonProperty("Satisfactory")]
    //    public string Satisfactory { get; set; }

    //    [JsonProperty("SML")]
    //    public string Sml { get; set; }

    //    [JsonProperty("Bootstrap")]
    //    public string Bootstrap { get; set; }

    //    [JsonProperty("Branch")]
    //    public string Branch { get; set; }

    //    public string GetEngine()
    //    {
    //        var engineSplit = Engine.Split("+++");
    //        return engineSplit[0].Substring(0, engineSplit[0].LastIndexOf("-"));
    //    }
    //    public string GetSFBuild()
    //    {
    //        var engineSplit = Engine.Split("+++");
    //        //4.22.3-125236
    //        //FactoryGame+rel-main-0.3.5

    //        var tmp = Satisfactory;
    //        tmp = tmp.TrimStart('+');
    //        //FactoryGame+rel-main-0.3.5-CL-125236
            
            
    //        tmp = tmp.Replace(engineSplit[1], "");
    //        //-CL-125236
            
    //        return tmp.TrimStart('-');
    //    }
    //    public string GetSFVersion()
    //    {
    //        var engineSplit = Engine.Split("+++");
    //        //4.22.3-125236
    //        //FactoryGame+rel-main-0.3.5

    //        var tmp = Satisfactory;
    //        tmp = tmp.TrimStart('+');
    //        //FactoryGame+rel-main-0.3.5-CL-125236
            
            
    //        var tmp2 = tmp.Replace(engineSplit[1], "");
    //        //-CL-125236

    //        tmp = tmp.Replace(tmp2, "");
    //        //FactoryGame+rel-main-0.3.5

    //        tmp = tmp.Substring(tmp.LastIndexOf("-") + 1);
            
    //        return tmp;
    //    }
    //}

    //public partial class Mod
    //{
    //    [JsonProperty("Modid")]
    //    public string Modid { get; set; }

    //    [JsonProperty("Name")]
    //    public string Name { get; set; }

    //    [JsonProperty("Version")]
    //    public string Version { get; set; }

    //    [JsonProperty("Desc")]
    //    public string Desc { get; set; }

    //    /// <summary>
    //    /// Split string at ;
    //    /// </summary>
    //    [JsonProperty("Authors")]
    //    public string Authors { get; set; }

    //    [JsonProperty("Credits")]
    //    public string Credits { get; set; }
    //}

    //public partial class World
    //{
    //    [JsonProperty("ServerName")]
    //    public string ServerName { get; set; }

    //    [JsonProperty("Players")]
    //    public long Players { get; set; }

    //    [JsonProperty("MaxPlayers")]
    //    public long MaxPlayers { get; set; }

    //    //enum ESessionVisibility
    //    //{
    //    //    SV_Private,         //UMETA(DisplayName=Private),
    //    //    SV_FriendsOnly,     //UMETA(DisplayName=FriendsOnly),
    //    //    SV_Invalid          //UMETA(Hidden)
    //    //};

    //    /// <summary>
    //    /// Is an Enum (SV_Private, SV_FriendsOnly, SV_Invalid)
    //    /// </summary>
    //    [JsonProperty("Visibility")]
    //    public string Visibility { get; set; }

    //    [JsonProperty("PlayTimeSeconds")]
    //    public long PlayTimeSeconds { get; set; }

    //    [JsonProperty("SessionName")]
    //    public string SessionName { get; set; }

    //    [JsonProperty("SessionID")]
    //    public string SessionId { get; set; }



    //    //enum ECachedNATType
    //    //{
    //    //    CNT_Open,           //UMETA(DisplayName="Open",     ToolTip="All peers can directly-connect to you"),
    //    //    CNT_Moderate,       //UMETA(DisplayName="Moderate", ToolTip="You can directly-connect to other Moderate and Open peers"),
    //    //    CNT_Strict,         //UMETA(DisplayName="Strict",   ToolTip="You can only directly-connect to Open peers"),
    //    //    CNT_TBD	            //UMETA(DisplayName="TBD",      ToolTip="NAT type has is not yet determined")
    //    //};

    //    /// <summary>
    //    /// Is an Enum (CNT_Open, CNT_Moderate, CNT_Strict, CNT_TBD)
    //    /// </summary>
    //    [JsonProperty("NatType")]
    //    public string NatType { get; set; }
    //}
}
