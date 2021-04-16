using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFServerManager.Code;

namespace SFSM_Watchdog.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class commandController : ControllerBase
    {
        private readonly ILogger<commandController> logger;
        public commandController(ILogger<commandController> _logger)
        {
            logger          = _logger;
        }

        [HttpPost]
        public async Task<JsonResult> PostAsync()
        {
            CommandResponse commandResponse = new CommandResponse();
            if (!this.Request.HasJsonContentType() || this.Request.Body == null)
            {
                this.Response.StatusCode = 400;
                commandResponse.Message  = "CommandResponse";
                commandResponse.Success  = false;
                commandResponse.Code     = 400;
                commandResponse.Error    = "Payload is not JSON";
                return new JsonResult(commandResponse)
                {
                    ContentType = "application/json; charset=utf-8",
                    SerializerSettings = new JsonSerializerOptions()
                    {
                        WriteIndented = false,
                        Encoder       = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }
                };
            }

            StreamReader sr   = new StreamReader(this.Request.Body);
            string       JSON = await sr.ReadToEndAsync();
            
            Console.WriteLine("I WAS POST REQUEST");
            Console.WriteLine("..");
            Console.WriteLine(JSON);
            Console.WriteLine("..");
            
            if (JSON == null || JSON.Length == 0)
            {
                this.Response.StatusCode = 400;
                commandResponse.Message  = "CommandResponse";
                commandResponse.Success  = false;
                commandResponse.Code     = 400;
                commandResponse.Error    = "Payload failed to read";
                return new JsonResult(commandResponse)
                {
                    ContentType = "application/json; charset=utf-8",
                    SerializerSettings = new JsonSerializerOptions()
                    {
                        WriteIndented = false,
                        Encoder       = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }
                };
            }

            

            this.Response.StatusCode  = 200;
            this.Response.ContentType = "application/json; charset=utf-8";
            CommandRequest cr = JsonConvert.DeserializeObject<CommandRequest>(JSON);

            if (cr != null)
            {
                commandResponse.Data.Request = cr;

                if (isSFSMCommand(cr.Command))
                {
                    if (Configuration.SFSMDetected())
                    {
                        //Forward to SFSM and get results
                        commandResponse = HttpHelper.PostCommandRequest(cr, GetURLForSFSM());
                        HandleObject(commandResponse, true,"Processor", "Processed by SFSM");
                    } else
                    {
                        HandleObject(commandResponse, false, "Error","SFSM Not Installed");
                    }

                } else
                {
                    //We Process it
                    if (cr.Command == "Stop" || cr.Command == "Force Quit")
                    {
                        Jobs.ProcessWatcher.DontRestart = true;
                    }
                    if (cr.Command == "Start" || cr.Command == "Restart")
                    {
                        Jobs.ProcessWatcher.DontRestart = false;
                    }

                    //Does SFSM also need to process it
                    if (isSFSMSupportingCommand(cr.Command))
                    {
                        //Forward to SFSM and get results
                        if (Configuration.SFSMDetected())
                        {
                            //Forward to SFSM and get results
                            commandResponse = HttpHelper.PostCommandRequest(cr, GetURLForSFSM());
                            HandleObject(commandResponse, true,"Processor", "Processed by SFSM+Watchdog");
                        } else
                        {
                            HandleObject(commandResponse, false, "Error","SFSM Not Installed");
                        }
                        
                        if (cr.Command == "Restart")
                        {
                            Configuration.EnsureStarted();
                            HandleObject(commandResponse, true,"Processor", "Processed by SFSM+Watchdog");
                        }
                    } else
                    {
                        if (cr.Command == "Start" )
                        {
                            Configuration.EnsureStarted();
                            HandleObject(commandResponse, true,"Processor", "Processed by Watchdog");
                        }
                        if (cr.Command == "Force Quit")
                        {
                            Configuration.TerminateGame();
                            HandleObject(commandResponse, true,"Processor", "Processed by Watchdog");
                        }
                    }
                }

            }
            else
            {
                this.Response.StatusCode = 400;
                commandResponse.Message = "CommandResponse";
                commandResponse.Success = false;
                commandResponse.Code = 400;
                commandResponse.Error = "Payload is not a Command Request";
                return new JsonResult(commandResponse)
                {
                    ContentType = "application/json; charset=utf-8",
                    SerializerSettings = new JsonSerializerOptions()
                    {
                        WriteIndented = false,
                        Encoder       = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }
                };
            }
           


            JsonResult jsonResult = new JsonResult(commandResponse)
            {
                ContentType = "application/json; charset=utf-8",
                SerializerSettings = new JsonSerializerOptions()
                {
                    WriteIndented = false,
                    Encoder       = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            };

            return jsonResult;
        }

        private string GetURLForSFSM()
        {
            return $"http://{Configuration.SFSMHostname()}:{Configuration.SFSM_Port}/command";
        }

        private bool isSFSMSupportingCommand(string command)
        {
            string[] SFSMCommands = new []{"Stop","Restart"};
            if (SFSMCommands.Contains(command))
                return true;

            return false;
        }

        private bool isSFSMCommand(string command)
        {
            string[] WatchDogCommands = new []{"Start","Stop","Restart","Force Quit"};
            if (WatchDogCommands.Contains(command))
                return false;

            return true;
        }

        //Add Watchdog Data
        private void HandleObject(CommandResponse commandResponse, bool hasSFSMOutput = true, string key = "", string value = "")
        {
            switch (key)
            {
                case "Processor":
                {
                    commandResponse.Data.Processor = value;
                    break;
                }
                case "Error":
                {
                    commandResponse.Data.Error = value;
                    break;
                }
            }
        }
    }
}
