using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFServerManager.Code.Static.Utilities
{
    public static class HttpHelper
    {
        public static CommandRequest CreateCommandRequest(string AccessSecret = "C366020887DF753F10B335D9E0A031D8", string command = "SaveGame", object ObjToSerialise = null)
        {
            CommandRequest cr = new CommandRequest();
            cr.Accesssecret = AccessSecret;
            cr.Command = command;

            if (ObjToSerialise != null)
            {
                StringWriter wr = new StringWriter();
                var settings = new JsonSerializerSettings();
                settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                (JsonSerializer.Create( settings )).Serialize(wr, ObjToSerialise);
                cr.Commandjsondata = wr.ToString();
            }
            
            return cr;
        }

        public static string PostCommandRequest(CommandRequest request, string url = "http://localhost:8033/command")
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            StringWriter wr = new StringWriter();
            JsonSerializer.Create().Serialize(wr, request);
            HttpContent content = new StringContent(wr.ToString());
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            
            var task = client.PostAsync(url, content).GetAwaiter().GetResult();
            string jsonResult = task.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(jsonResult);

            return jsonResult;
        }

    }
}
