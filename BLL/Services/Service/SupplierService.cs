
using BLL.Services.IService;
using DAL.Repositories.IRepository;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unit;

        public SupplierService(IUnitOfWork unitOfWork)
        {
            _unit = unitOfWork;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(Expression<Func<Supplier, bool>> filter = null)
        {
            return await _unit.Supplier.GetAllAsync(filter);
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            return await _unit.Supplier.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        } 
        public async Task<Supplier> GetByNameAsync(string name)
        {
            return await _unit.Supplier.GetFirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<Supplier> CreateAsync(Supplier supplier)
        {
            _unit.Supplier.Add(supplier);
            _unit.CompleteAsync();
            return supplier;
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _unit.Supplier.Update(supplier); 
            _unit.CompleteAsync();
            return supplier;
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _unit.Supplier.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (supplier != null)
            {
                //_unit.Supplier.Remove(supplier);
                supplier.IsDeleted = true;
                _unit.CompleteAsync();
            }
        }
    }
}
