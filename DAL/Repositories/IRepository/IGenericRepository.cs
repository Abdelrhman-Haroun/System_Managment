using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repositories.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        void Add(T entity);                               
        Task AddAsync(T entity);                       
        Task AddRangeAsync(IEnumerable<T> entities);       
        Task<T?> GetByIdAsync(int id);          
        Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null);                   
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null);                
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate);      
        void Update(T entity);                            
        void Remove(T entity);                         
        void RemoveRange(IEnumerable<T> entities);       
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);
    }

}
