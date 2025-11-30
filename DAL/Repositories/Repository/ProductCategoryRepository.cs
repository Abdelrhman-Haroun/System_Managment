
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.Repositories;
using System;

namespace DAL.Repositories.IRepository
{
    public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductCategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
