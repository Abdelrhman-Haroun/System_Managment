
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.Repositories;
using System;

namespace DAL.Repositories.IRepository
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        private readonly ApplicationDbContext _context;
        public StoreRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
