using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebBankingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Home : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<Home> _logger;

        public Home(ILogger<Home> logger)
        {
            _logger = logger;
        }

        
    }
}
