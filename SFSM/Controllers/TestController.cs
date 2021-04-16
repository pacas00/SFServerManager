using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using SFServerManager.Code.Instanced.Services;

namespace SFServerManager.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DatabaseService DatabaseService;
        private readonly ILogger<TestController> logger;
        public TestController(DatabaseService _DatabaseService, ILogger<TestController> _logger)
        {
            DatabaseService = _DatabaseService;
            logger = _logger;
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            Console.WriteLine("I WAS REQUEST");
            this.Response.StatusCode = 200;
            this.Response.ContentType = "application/json; charset=utf-8";

            JObject o = new JObject();
            o["Key"] = "123";

            string result = JsonConvert.SerializeObject(o, new JsonSerializerSettings()
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                Formatting = Formatting.None
            });
            JsonResult jsonResult = new JsonResult(result)
            {
                ContentType = "application/json; charset=utf-8",
                SerializerSettings = new JsonSerializerOptions()
                {
                    WriteIndented = false,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            };

            return jsonResult;
        }

        [HttpPost]
        public async Task<JsonResult> PostAsync()
        {
            StreamReader sr = new StreamReader(this.Request.Body);
            string JSON = await sr.ReadToEndAsync();

            Console.WriteLine("I WAS POST REQUEST " + JSON);

            this.Response.StatusCode = 200;
            this.Response.ContentType = "application/json; charset=utf-8";

            JObject o = new JObject();
            o["Key"] = "123";

            string result = JsonConvert.SerializeObject(o, new JsonSerializerSettings()
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                Formatting = Formatting.None
            });
            JsonResult jsonResult = new JsonResult(result)
            {
                ContentType = "application/json; charset=utf-8",
                SerializerSettings = new JsonSerializerOptions()
                {
                    WriteIndented = false,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }
            };

            return jsonResult;
        }
    }
}