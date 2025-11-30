
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.Repositories;
using System;

namespace DAL.Repositories.IRepository
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        private readonly ApplicationDbContext _context;
        public SupplierRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
