using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentScheduler;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SFServerManager.Code.Instanced.Services;
using SFServerManager.Code.Static.Utilities;

namespace SFServerManager.Code.JobSystem
{
    public class ServerStatusChecker : IJob
    {
        private DatabaseService _DB;

        public ServerStatusChecker(DatabaseService db)
        {
            this._DB = db;
        }

        public void Execute()
        {
            IEnumerable<ServerEntry> Entries = _DB.FindAll<ServerEntry>("Servers");

            bool isRetry = false;

            foreach (ServerEntry entry in Entries)
            {
                retry:
                try
                {
                    if (entry.SFSM_Port != -1)
                    {
                        HttpClient client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(30);
                       
                        string result = client.GetStringAsync(entry.buildURL("status")).GetAwaiter().GetResult();
                        StatusResponse response = JsonSerializer.Create().Deserialize<StatusResponse>(new JsonTextReader(new StringReader(result)));

                        if (response.Success == false) continue;

                        if (entry.LastStatus == null)
                        {
                            entry.LastStatus = response.Data;
                        } else
                        {
                            entry.LastStatus.Merge(response.Data);
                        }
                        _DB.Update("Servers", entry);
                        //http://localhost:8033/status

                    }
                    isRetry = false;
                }
                catch (HttpRequestException httpRequestException)
                {
                    if (httpRequestException.Message.Contains("target machine actively refused"))
                    {
                        //Skip
                        continue;
                    } 
                    else if (httpRequestException.Message.Contains("established connection was aborted by the software in your host machine"))
                    {
                        //Damn it.
                        if (!isRetry)
                        {
                            isRetry = true;
                            goto retry;
                        } else
                        {
                            isRetry = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine(httpRequestException);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

                if (entry.LastStatus.World.MapType == "GameWorld")
                {
                    //Cool, We have completed status, I hope
                    var req         = HttpHelper.CreateCommandRequest(entry.Apikey, "ListPlayers", "");
                    var cmdResponse = HttpHelper.PostCommandRequest(req, entry.buildURL("command"));
                    if (cmdResponse?.Data?.Response?.Players != null)
                    {
                        entry.LastStatus.Players = cmdResponse?.Data?.Response?.Players;
                        _DB.Update("Servers", entry);
                    }
                }
            }
        }
    }
}