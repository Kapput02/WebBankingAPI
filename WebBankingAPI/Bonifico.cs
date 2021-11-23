using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebBankingAPI
{
    public class Bonifico 
    {
        public string Iban { get; set; }
        public int Importo { get; set; }

    }
}
