using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.EmployeeType;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Service
{
    public class EmployeeTypeService : IEmployeeTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeTypeService> _logger;

        public EmployeeTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EmployeeTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeType>> GetAllAsync(string? searchTerm = null)
        {
            var items = await _unitOfWork.EmployeeType.GetAllAsync(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                items = items.Where(x =>
                    x.Name.ToLower().Contains(term) ||
                    (!string.IsNullOrWhiteSpace(x.Description) && x.Description.ToLower().Contains(term)));
            }

            return items.OrderBy(x => x.Name).ToList();
        }

        public async Task<EmployeeType?> GetByIdAsync(int id)
        {
            return await _unitOfWork.EmployeeType.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<EmployeeType> CreateAsync(CreateEmployeeTypeVM model)
        {
            var normalizedName = model.Name.Trim().ToLower();
            var exists = await _unitOfWork.EmployeeType.AnyAsync(x => !x.IsDeleted && x.Name.ToLower() == normalizedName);
            if (exists)
            {
                throw new InvalidOperationException("يوجد نوع موظف بنفس الاسم");
            }

            var entity = _mapper.Map<EmployeeType>(model);
            _unitOfWork.EmployeeType.Add(entity);
            await _unitOfWork.CompleteAsync();
            return entity;
        }

        public async Task<EmployeeType> UpdateAsync(EditEmployeeTypeVM model)
        {
            var entity = await _unitOfWork.EmployeeType.GetFirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);
            if (entity == null)
            {
                throw new InvalidOperationException("نوع الموظف غير موجود");
            }

            var normalizedName = model.Name.Trim().ToLower();
            var exists = await _unitOfWork.EmployeeType.AnyAsync(x => !x.IsDeleted && x.Id != model.Id && x.Name.ToLower() == normalizedName);
            if (exists)
            {
                throw new InvalidOperationException("هذا الاسم مستخدم في نوع موظف آخر");
            }

            entity.Name = model.Name.Trim();
            entity.Description = model.Description?.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.EmployeeType.Update(entity);
            await _unitOfWork.CompleteAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.EmployeeType.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (entity == null)
            {
                return false;
            }

            var isUsed = await _unitOfWork.Employee.AnyAsync(x => !x.IsDeleted && x.EmployeeTypeId == id);
            if (isUsed)
            {
                throw new InvalidOperationException("لا يمكن حذف نوع موظف مرتبط بموظفين");
            }

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.EmployeeType.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
