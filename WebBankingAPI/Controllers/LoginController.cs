using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebBankingAPI.Models;

namespace WebBankingAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost("")]
        public ActionResult Login([FromBody] User credentials)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                User candidate = model.Users.FirstOrDefault(q => q.Username == credentials.Username && q.Password == credentials.Password);

                if (candidate == null) return Unauthorized();

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    SigningCredentials = new SigningCredentials(SecurityKeyGenerator.GetSecurityKey(), SecurityAlgorithms.HmacSha256),
                    Expires = DateTime.UtcNow.AddDays(1),
                    Subject = new ClaimsIdentity
                    (
                        new Claim[]
                        {
                            new Claim("id", candidate.Id.ToString())
                        }
                    )
                };
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                candidate.LastLogin = DateTime.UtcNow;
                model.SaveChanges();
                return Ok(tokenHandler.WriteToken(token));
            }
        }
        [Authorize]
        [HttpPost("/api/Logout")]
        public ActionResult Logout()
        {

            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                candidate.LastLogout = DateTime.UtcNow;
                model.SaveChanges();
                return Ok("Logout avvenuto con successo. La sessione è durata "+ (candidate.LastLogout-candidate.LastLogin));
            }
        }
    }
}
