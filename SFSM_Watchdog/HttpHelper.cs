using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFServerManager.Code;
using SFSM_Watchdog.Controllers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SFSM_Watchdog
{
    public static class HttpHelper
    {
        public static CommandRequest CreateCommandRequest(string AccessSecret, string command, object ObjToSerialise = null)
        {
            CommandRequest cr = new CommandRequest();
            cr.Accesssecret = AccessSecret;
            cr.Command = command;
            cr.Commandjsondata = "";

            if (ObjToSerialise != null)
            {
                StringWriter wr = new StringWriter();
                var settings = new JsonSerializerSettings();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                (JsonSerializer.Create(settings)).Serialize(wr, ObjToSerialise);
                cr.Commandjsondata = wr.ToString();
            }

            return cr;
        }

        public static CommandResponse PostCommandRequest(CommandRequest request, string url = "http://localhost:8033/command")
        {
            bool isRetry = false;
            //Ok!
            retry:
            try
            {
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(15);
                    StringWriter wr = new StringWriter();
                    JsonSerializer.Create().Serialize(wr, request);
                    HttpContent content = new StringContent(wr.ToString());
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var task = client.PostAsync(url, content).GetAwaiter().GetResult();
                    string jsonResult = task.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    #if DEBUG
                    Console.WriteLine("---");
                    Console.WriteLine(jsonResult);
                    Console.WriteLine("---");
                    #endif
                    return JsonConvert.DeserializeObject<CommandResponse>(jsonResult, new JsonSerializerSettings()
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                        Formatting = Formatting.None
                    });
                    
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
                    Console.WriteLine("Failed to send request to " + url);
                    //Damn it.
                    if (!isRetry)
                    {
                        isRetry = true;
                        goto retry;
                    }
                    else
                    {
                        isRetry = false;
                    }
                }
                else if (httpRequestException.Message.Contains("established connection was aborted by the software in your host machine"))
                {
                    //Damn it.
                    if (!isRetry)
                    {
                        isRetry = true;
                        goto retry;
                    }
                    else
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
            return CommandResponse.Failed();

        }

    }
}
