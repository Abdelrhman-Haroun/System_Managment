using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CashBox> CashBoxes { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<ProductTransaction> ProductTransactions { get; set; }
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
        public DbSet<SupplierTransaction> SupplierTransactions { get; set; }
        public DbSet<InternalProductUsage> InternalProductUsages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)
            );
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .Property(x => x.StockQuantity)
                .HasPrecision(18, 2);

            builder.Entity<ProductTransaction>(entity =>
            {
                entity.Property(x => x.QuantityBefore).HasPrecision(18, 2);
                entity.Property(x => x.QuantityChanged).HasPrecision(18, 2);
                entity.Property(x => x.WeightChanged).HasPrecision(18, 2);
                entity.Property(x => x.QuantityAfter).HasPrecision(18, 2);
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            });

            builder.Entity<CustomerTransaction>(entity =>
            {
                entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
                entity.Property(x => x.AmountChanged).HasPrecision(18, 2);
                entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            });

            builder.Entity<SupplierTransaction>(entity =>
            {
                entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
                entity.Property(x => x.AmountChanged).HasPrecision(18, 2);
                entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            });

            // Seed roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "SuperAdmin", NormalizedName = "SUPERADMIN" },
                new IdentityRole { Id = "2", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "3", Name = "Employee", NormalizedName = "EMPLOYEE" }
            );

            // Seed admin
            var adminId = "admin-user-id-12345";

            var adminUser = new ApplicationUser
            {
                Id = adminId,
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "System Administrator",
                LockoutEnabled = true
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Add admin to SuperAdmin role
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminId,
                    RoleId = "1"
                }
            );
        }

    }
}
