
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.Repositories;
using System;

namespace DAL.Repositories.IRepository
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
