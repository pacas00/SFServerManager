using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    public class statusController : ControllerBase
    {
        private readonly ILogger<statusController> logger;
        public statusController(ILogger<statusController> _logger)
        {
            logger          = _logger;
        }

        [HttpGet]
        public async Task<JsonResult> GetAsync()
        {
            StatusResponse statusResponse = new StatusResponse();
            if (Configuration.SFSM_Installed)
            {
                bool isRetry = false;
                //Ok!
                retry:
                try
                {
                    {
                        HttpClient client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(15);
                       
                        string Status = await client.GetStringAsync(GetURLForSFSM());
                        
                        Console.WriteLine(".");
                        Console.WriteLine(Status);
                        Console.WriteLine(".");

                        StatusResponse cr = JsonConvert.DeserializeObject<StatusResponse>(Status, new JsonSerializerSettings()
                        {
                            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                            Formatting           = Formatting.None
                        });

                        //Do what we need to
                        //TODO: INTO HANDLER
                        this.Response.StatusCode = 200;

                        HandleObject(cr);

                        return new JsonResult(cr)
                        {
                            ContentType = "application/json; charset=utf-8",
                            SerializerSettings = new JsonSerializerOptions()
                            {
                                WriteIndented = true,
                                Encoder       = JavaScriptEncoder.Default
                            }
                        };


                        //http://localhost:8033/status

                    }
                    isRetry = false;
                }
                catch (HttpRequestException httpRequestException)
                {
                    if (httpRequestException.Message.Contains("target machine actively refused"))
                    {
                        
                    } 
                    else if (httpRequestException.Message.Contains("An error occurred while sending the request"))
                    {
                        Console.WriteLine("Failed to send request to " + GetURLForSFSM());
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

            }
            

            this.Response.StatusCode  = 200;
            this.Response.ContentType = "application/json; charset=utf-8";

            //output 
            //JObject o = new JObject();
           
            HandleObject(statusResponse, false);

            JsonResult jsonResult = new JsonResult(statusResponse)
            {
                ContentType = "application/json; charset=utf-8",
                SerializerSettings = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder       = JavaScriptEncoder.Default
                }
            };

            return jsonResult;
        }

        //Add Watchdog Data
        private void HandleObject(StatusResponse statusResponse, bool hasSFSMOutput = true)
        {
        }

        private string GetURLForSFSM()
        {
            return $"http://{Configuration.SFSMHostname()}:{Configuration.SFSM_Port}/status";
        }
    }
}
