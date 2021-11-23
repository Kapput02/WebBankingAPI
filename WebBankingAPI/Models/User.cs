using System;
using System.Collections.Generic;

#nullable disable

namespace WebBankingAPI.Models
{
    public partial class User
    {
        public User()
        {
            BankAccounts = new HashSet<BankAccount>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public bool IsBanker { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastLogout { get; set; }

        public virtual ICollection<BankAccount> BankAccounts { get; set; }
    }
}
