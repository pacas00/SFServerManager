using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFServerManager.Code;
using SFServerManager.Code.DataObjects;
using SFServerManager.Code.Instanced.Services;

namespace SFServerManager.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServerClientController : ControllerBase
    {
        private readonly DatabaseService DatabaseService;
        private readonly ILogger<ServerClientController> logger;
        public ServerClientController(DatabaseService _DatabaseService, ILogger<ServerClientController> _logger)
        {
            DatabaseService = _DatabaseService;
            logger = _logger;
        }

        
    }
}
