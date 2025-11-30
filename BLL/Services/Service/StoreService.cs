
using BLL.Services.IService;
using DAL.Repositories.IRepository;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class StoreService : IStoreService
    {
        private readonly IUnitOfWork _unit;

        public StoreService(IUnitOfWork unitOfWork)
        {
            _unit = unitOfWork;
        }

        public async Task<IEnumerable<Store>> GetAllAsync(Expression<Func<Store, bool>> filter = null)
        {
            return await _unit.Store.GetAllAsync(filter);
        }

        public async Task<Store> GetByIdAsync(int id)
        {
            return await _unit.Store.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        } 
        public async Task<Store> GetByNameAsync(string name)
        {
            return await _unit.Store.GetFirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<Store> CreateAsync(Store Store)
        {
            _unit.Store.Add(Store);
            _unit.Complete();
            return Store;
        }

        public async Task<Store> UpdateAsync(Store Store)
        {
            _unit.Store.Update(Store); 
            _unit.Complete();
            return Store;
        }

        public async Task DeleteAsync(int id)
        {
            var Store = await _unit.Store.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (Store != null)
            {
                //_unit.Store.Remove(Store);
                Store.IsDeleted = true;
                _unit.Complete();
            }
        }
    }
}
