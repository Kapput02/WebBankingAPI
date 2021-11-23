using System;
using System.Collections.Generic;

#nullable disable

namespace WebBankingAPI.Models
{
    public partial class BankAccount
    {
        public BankAccount()
        {
            AccountMovements = new HashSet<AccountMovement>();
        }

        public int Id { get; set; }
        public string Iban { get; set; }
        public int? FkUser { get; set; }

        public virtual User FkUserNavigation { get; set; }
        public virtual ICollection<AccountMovement> AccountMovements { get; set; }
    }
}
