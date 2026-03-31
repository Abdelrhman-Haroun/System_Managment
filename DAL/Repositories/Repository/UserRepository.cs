
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DAL.Models;
using DAL.Data;
using DAL.Repositories;
using System;
using Microsoft.AspNetCore.Identity;

namespace DAL.Repositories.IRepository
{
public class UserRepository(UserManager<ApplicationUser> _userManager) : IUserRepository
{
        public async Task<ApplicationUser> GetByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync(Expression<Func<ApplicationUser, bool>> filter = null)
        {
            var query = _userManager.Users.Where(u => !u.IsDeleted);

            if (filter != null)
                query = query.Where(filter);

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _userManager.Users.AnyAsync(predicate);
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded ? user : null;
        }

        public async Task<bool> UpdateAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            user.IsDeleted = true;
            return await UpdateAsync(user);
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> AddToRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<bool> IsLockedOutAsync(ApplicationUser user)
        {
            return await _userManager.IsLockedOutAsync(user);
        }

        public async Task<bool> ToggleLockoutAsync(string id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                user.LockoutEnd = null;
            else
                user.LockoutEnd = DateTime.UtcNow.AddYears(100);

            return await UpdateAsync(user);
        }

        public async Task UpdateSecurityStampAsync(ApplicationUser user)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }
        public async Task<ApplicationUser> GetActiveUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.IsDeleted)
                return null;

            return user;
        }
    }
}

