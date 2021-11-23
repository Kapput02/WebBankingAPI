using System;
using System.Collections.Generic;

#nullable disable

namespace WebBankingAPI.Models
{
    public partial class AccountMovement
    {
        
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int FkBankAccount { get; set; }
        public double? In { get; set; }
        public double? Out { get; set; }
        public string Description { get; set; }

        public virtual BankAccount FkBankAccountNavigation { get; set; }
    }
}
