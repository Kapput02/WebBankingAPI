using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Models;
using Microsoft.EntityFrameworkCore;

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
            using ( WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                var conti = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).ToList();
                if (conti.Count == 0) return NotFound("Non esiste alcun conto corrente");
                if (candidate.IsBanker)
                {
                    return Ok(conti);
                }
                var contiUtente = model.BankAccounts.Select(s=> new { s.Id, s.Iban,s.FkUser }).Where( i => i.FkUser == Int32.Parse(idUtente)).ToList();
                if (contiUtente.Count == 0) return NotFound("Non possiedi alcun conto corrente");
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
                return Unauthorized("Non ha i diritti per accedere al conto con ID a"+conto.Id);
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
                return Unauthorized("Non hai accesso ai movimenti di questo conto");
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
                if (conto == null) return NotFound("Conto non esistente");
                if (conto.FkUser != int.Parse(idUtente) && candidate.IsBanker == false) return Unauthorized("Non hai l'accesso a questo conto");
                var movimenti = model.AccountMovements.Select(s => new { s.Id, s.Date, s.Description, s.FkBankAccount }).Where(i => i.FkBankAccount == conto.Id).ToList();
                var movimento = movimenti.Select(s => new { s.Id, s.Date, s.Description, s.FkBankAccount }).FirstOrDefault(i => i.Id == id2);
                if (movimento == null) return NotFound("Movimento non esistente su questo conto");
                return Ok(movimento);
            }

        }
        [HttpPost]
        [Route("{id}/bonifico")]
        [Authorize]
        public ActionResult Bonifico(int id,[FromBody] Bonifico bonifico)
        {
            if (bonifico.Importo <= 0) return Problem("La cifra inserita non è valida");
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;//estraggo idUtente da claim
                User candidate = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));//estraggo utente
                var contoInviante = model.BankAccounts.Select(s => new { s.Id, s.Iban, s.FkUser }).FirstOrDefault(i => i.Id == id);//estraggo conto che fa il bonifico
                if (contoInviante == null) return NotFound("Conto inserito non valido");
                if (contoInviante.Iban == bonifico.Iban) return Problem("L'iban del mittente e del destinatario del bonifico devono essere diversi ");
                if (candidate.IsBanker || contoInviante.FkUser == int.Parse(idUtente))
                {                
                    var movimenti = model.AccountMovements.Select(s => new { s.Id, s.Date, s.Description, s.In,s.Out,s.FkBankAccount }).Where(i => i.FkBankAccount == contoInviante.Id).OrderBy(a => a.Date).ToList();
                    double? saldo = movimenti.Sum(i => (i.In == null) ? 0 : i.In);
                    saldo -= movimenti.Sum(i => (i.In == null) ? i.Out : 0);
                    if (saldo == null) return Problem();
                    if ((saldo - bonifico.Importo) < 0) return Problem("Saldo insufficiente");
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
                return Unauthorized("Il conto inserito non le appartiene");
            }

        }
        [HttpPost]
        [Route("")]
        [Authorize]
        public ActionResult CreaConto([FromBody] BankAccount conto)
        {
            if (conto == null) return NotFound("Non hai inserito i parametri necessari alla creazione del conto");
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
                        return Ok("Il conto con iban "+conto.Iban+" e ID "+ conto.Id+" è stato creato correttamente");
                    }
                    else
                    {
                        return Problem("Hai inserito un iban già esistente");
                    }
                        
                    }
                    catch
                    {
                        return Problem("Controlla di aver inserito correttamente l'Iban e l'Id dell'intestatario del conto");
                    }
                
            }

        }
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult Update(int id, [FromBody] BankAccount contoAggiornato)
            { 
            using ( WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User utente = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                if (!utente.IsBanker)
                {
                    return Unauthorized("Non hai i diritti necessari per compiere questa operazione");
                }
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
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User utente = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                if (!utente.IsBanker)
                {
                    return Unauthorized("Non hai i diritti necessari per compiere questa operazione");
                }
                BankAccount contoCandidato = model.BankAccounts.FirstOrDefault(o => o.Id == id);
                List<AccountMovement> movimenti = model.AccountMovements.Where(i => i.FkBankAccount == id).ToList();
                if (contoCandidato == null) return NotFound("Conto non esistente");
                model.AccountMovements.RemoveRange(movimenti);
                model.BankAccounts.Remove(contoCandidato);
                model.SaveChanges();
                return Ok("Il conto con id "+ id + " è stato cancellato correttamente");
            }
        }
        [HttpGet("{id}/saldo")]
        [Authorize]
        public ActionResult Saldo(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var idUtente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User utente = model.Users.FirstOrDefault(q => q.Id == Int32.Parse(idUtente));
                BankAccount candidate = model.BankAccounts.Include(i=>i.AccountMovements).FirstOrDefault(o => o.Id == id);
                if (candidate == null) return NotFound("Il conto selezionato non esiste");
                if (utente.IsBanker || utente.Id == candidate.FkUser)
                {
                    double? saldo = candidate.AccountMovements.Sum(i => (i.In == null) ? 0 : i.In);
                    saldo -= candidate.AccountMovements.Sum(i => (i.In == null) ? i.Out : 0);
                    return Ok(saldo);
                }
                return Unauthorized("Questo conto non ti appartiene"); 
            }
        }
    }
}
