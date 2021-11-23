using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public ActionResult Get()
        {
            using (WebBankingContext model = new WebBankingContext())
            {

                List<User> candidati = model.Users.ToList();
                if (candidati == null) return NotFound();
                return Ok(candidati);

            }
        }
    }
}
