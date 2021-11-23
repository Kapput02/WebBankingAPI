using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("api/conti-correnti")]
    [ApiController]
    public class contiCorrentiController : ControllerBase
    {

        [HttpGet]
        [Route("")]
        [Authorize]
        public ActionResult Get()
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                var conti = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).ToList();
                if (conti == null) return NotFound();
                if (candidate.IsBanker)
                {
                    return Ok(conti);
                }
                var contiUtente = model.BankAccounts.Select(s=> new { s.Id, s.Iban,s.FkUser }).Where( i => i.FkUser == Int32.Parse(idUtente)).ToList();
                return Ok(contiUtente);
            }
            
        }
        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public ActionResult GetOne(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                var conto = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i=> i.Id ==id );
                if (conto == null) return NotFound();
                if (candidate.IsBanker || conto.FkUser == int.Parse(idUtente))
                {
                    return Ok(conto);
                }
                return NotFound(conto.Id);
            }

        }
        [HttpGet]
        [Route("{id}/movimenti")]
        [Authorize]
        public ActionResult GetMovimenti(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                var conto = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Id == id);
                var movimenti = model.AccountMovements.Select(s => new { s.Id, s.Date, s.Description, s.FkBankAccount }).Where(i => i.FkBankAccount == conto.Id).OrderBy(a=> a.Date).ToList();
                if (conto == null) return NotFound();
                if (candidate.IsBanker || conto.FkUser == int.Parse(idUtente))
                {
                    return Ok(movimenti);
                }
                return NotFound(conto.Id);
            }

        }
        [HttpGet]
        [Route("{id}/movimenti/{id2}")]
        [Authorize]
        public ActionResult GetMovimento(int id,int id2)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                var conto = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Id == id);
                var movimenti = model.AccountMovements.Select(s => new { s.Id, s.Date, s.Description, s.FkBankAccount }).Where(i => i.FkBankAccount == conto.Id).ToList();
                var movimento = model.AccountMovements.Select(s => new { s.Id, s.Date, s.Description, s.FkBankAccount }).FirstOrDefault(i => i.FkBankAccount == id2);
                if (conto == null) return NotFound();
                if (movimento == null) return NotFound();
                if (candidate.IsBanker || conto.FkUser == int.Parse(idUtente))
                {
                    return Ok(movimento);

                }
                return NotFound(conto.Id);
            }

        }
        [HttpPost]
        [Route("{id}/bonifico")]
        [Authorize]
        public ActionResult Bonifico(int id,[FromBody] Bonifico bonifico)
        {
            if (bonifico.Importo < 0) return Problem("La cifra inserita è negativa");
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                var contoInviante = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Id == id);
                if (contoInviante == null) return NotFound("Conto inserito non valido");
                if (candidate.IsBanker || contoInviante.FkUser == int.Parse(idUtente))
                {
                    AccountMovement movimentoInviato = new AccountMovement { Date = DateTime.UtcNow, Out = bonifico.Importo, Description = "Bonifico inviato", FkBankAccount = contoInviante.Id };
                    var contoRicevente = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Iban == bonifico.Iban);
                    if (contoRicevente != null)
                    {
                        AccountMovement movimentoRicevuto = new AccountMovement { Date = DateTime.UtcNow, In = bonifico.Importo, Description = "Bonifico ricevuto", FkBankAccount = contoRicevente.Id };
                        model.AccountMovements.Add(movimentoRicevuto);
                        model.SaveChanges();
                    }
                    model.AccountMovements.Add(movimentoInviato);
                    model.SaveChanges();
                    return Ok(movimentoInviato);
                }
                return Unauthorized("Conto inserito non le appartiene");
            }

        }
        [HttpPost]
        [Route("")]
        [Authorize]
        public ActionResult CreaConto([FromBody] BankAccount conto)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                if (!candidate.IsBanker)
                {
                    return Unauthorized("Non hai i diritti necessari per compiere questa operazione");
                }
               
                    try
                    {
                        if(model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Iban == conto.Iban) == null)
                    {
                        model.BankAccounts.Add(conto);
                        model.SaveChanges();
                        return Ok(conto);
                    }
                    else
                    {
                        return Problem("Hai inserito un iban già esistente");
                    }
                        
                    }
                    catch
                    {
                        return Problem("Controlla di aver inserito un FkUser e un iban validi");
                    }
                
            }

        }
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult Update(int id, [FromBody] BankAccount contoAggiornato)
            { 
            using ( WebBankingContext model = new WebBankingContext())
            {
                BankAccount candidate = model.BankAccounts.FirstOrDefault(o => o.Id == id);
                if (candidate == null) return NotFound("Conto non esistente");
                if (id != candidate.Id) return BadRequest();
                if (model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Iban == contoAggiornato.Iban) != null) return BadRequest("Scegliere un iban non presente nel database");
                candidate.Iban = contoAggiornato.Iban;
                model.SaveChanges();
                return Ok(candidate);
            }
        }
        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult Delete(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                BankAccount candidate = model.BankAccounts.FirstOrDefault(o => o.Id == id);
                List<AccountMovement> movimenti = model.AccountMovements.Where(i => i.FkBankAccount == id).ToList();
                if (candidate == null) return NotFound();
                
                model.AccountMovements.RemoveRange(movimenti);
                model.BankAccounts.Remove(candidate);
                model.SaveChanges();
                return Ok();
            }
        }
    }
}
