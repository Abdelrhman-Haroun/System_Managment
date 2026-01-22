
using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DAL.Repositories.IRepository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Product?> GetByIdContainsAsync(
         int id,
         string includes)
        {
            IQueryable<Product> query = _context.Products;

            if (includes != null)
            {
                foreach (var item in includes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(item);
            }
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
