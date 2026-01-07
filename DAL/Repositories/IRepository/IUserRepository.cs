using DAL.Models;
using System.Linq.Expressions;

namespace DAL.Repositories.IRepository
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<IEnumerable<ApplicationUser>> GetAllAsync(Expression<Func<ApplicationUser, bool>> filter = null);
        Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate);
        Task<ApplicationUser> CreateAsync(ApplicationUser user, string password);
        Task<bool> UpdateAsync(ApplicationUser user);
        Task<bool> SoftDeleteAsync(string id);
        Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> AddToRoleAsync(ApplicationUser user, string role);
        Task<bool> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);
        Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<bool> IsLockedOutAsync(ApplicationUser user);
        Task<bool> ToggleLockoutAsync(string id);
        Task UpdateSecurityStampAsync(ApplicationUser user);
    }
}
