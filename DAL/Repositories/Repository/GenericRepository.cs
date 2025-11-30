using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repositories.IRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            if (includeWord != null)
            {
                foreach (var item in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(item);
            }

            return await query.ToListAsync();
        }

        public async Task<T> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            if (includeWord != null)
            {
                foreach (var item in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(item);
            }

            return await query.FirstOrDefaultAsync();
        }

        public void Add(T entity) => _dbSet.Add(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Remove(T entity) => _dbSet.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
    }

}
