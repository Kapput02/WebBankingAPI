using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace WebBankingAPI.Models
{
    public partial class WebBankingContext : DbContext
    {
        public WebBankingContext()
        {
        }

        public WebBankingContext(DbContextOptions<WebBankingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccountMovement> AccountMovements { get; set; }
        public virtual DbSet<BankAccount> BankAccounts { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=DESKTOP-LL4DG0N\\SQLEXPRESSNEW;Database=WebBanking;Trusted_connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<AccountMovement>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.FkBankAccount).HasColumnName("fk_bankAccount");

                entity.Property(e => e.In).HasColumnName("in");

                entity.Property(e => e.Out).HasColumnName("out");

                entity.HasOne(d => d.FkBankAccountNavigation)
                    .WithMany(p => p.AccountMovements)
                    .HasForeignKey(d => d.FkBankAccount)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccountMovements_BankAccounts");
            });

            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FkUser).HasColumnName("fk_user");

                entity.Property(e => e.Iban)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("iban");

                entity.HasOne(d => d.FkUserNavigation)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.FkUser)
                    .HasConstraintName("FK_BankAccounts_BankAccounts");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.IsBanker).HasColumnName("is_banker");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("last_login");

                entity.Property(e => e.LastLogout)
                    .HasColumnType("datetime")
                    .HasColumnName("last_logout");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("username");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
