
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.Repositories;
using System;

namespace DAL.Repositories.IRepository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
