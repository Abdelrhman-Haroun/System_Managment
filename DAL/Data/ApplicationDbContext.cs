using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
     {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {

            }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Worker> Workers { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            List<IdentityRole> roles = new List<IdentityRole>
             {
                new IdentityRole
                {
                  Id = "1f0aa99c-8cfa-4d9f-aefd-50b6a5f5e0b1",
                  Name = "SuperAdmin",
                  NormalizedName = "SUPERADMIN"
                }
                ,
               new IdentityRole
               {
                 Id = "2f0aa99c-8cfa-4d9f-aefd-50b6a5f5e0b2",
                 Name = "Admin",
                 NormalizedName = "ADMIN"
               },
               new IdentityRole
               {
                 Id = "3f0aa99c-8cfa-4d9f-aefd-50b6a5f5e0b3",
                 Name = "Employee",
                 NormalizedName = "EMPLOYEE"
               },
             };
            builder.Entity<IdentityRole>().HasData(roles);

        }
    }
}



