
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.IRepository;
using System;

namespace DAL.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<ICollection<Product>> GetDefaultProducts(Expression<Func<Product, bool>> filter = null)
        {
            if (filter != null)

                // .include("entity")   we do eagger load
                return await _context.Products.Where(filter).Include("Reviews").Include("SubCategory").ToListAsync();
            else
                return await _context.Products.ToListAsync();


        }

        public async Task<Product> GetProductById(Expression<Func<Product, bool>> filter = null)
        {
            return await _context.Products.Where(filter).FirstOrDefaultAsync();
        }

        public async Task<ICollection<Product>> GetProductsByCategory(Expression<Func<Product, bool>> filter = null)
        {
           if(filter != null)
            {
                return await _context.Products.Where(filter).ToListAsync();

            }else
            {
                return await _context.Products.ToListAsync();
            }
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await  _context.SaveChangesAsync();
            return await _context.Products.OrderByDescending(x => x.Price).FirstOrDefaultAsync();
        }

        public async Task<ICollection<Product>> GetAllProductsAsync(Expression<Func<Product, bool>> filter = null)
        {
            if(filter != null)
        return await _context.Products.Where(filter).ToListAsync();
            else return await _context.Products.ToListAsync();
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.Products.FindAsync(product.Id);
            await _context.SaveChangesAsync();
            return await _context.Products.FindAsync(product.Id);
        }
        public async Task DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            await _context.SaveChangesAsync();

        }

    }
}
