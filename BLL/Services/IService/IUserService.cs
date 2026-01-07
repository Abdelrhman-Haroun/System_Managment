using BLL.ViewModels.Account;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IUserService
    {
        Task<(bool Success, string Message, IEnumerable<ApplicationUser> Users)> GetAllUsersAsync(string searchTerm);
        Task<(bool Success, ApplicationUser User, string Role, DateTime? CreatedAt)> GetUserDetailsAsync(string id);
        Task<(bool Success, string Message, EditUserVM Model)> GetUserForEditAsync(string id);
        Task<(bool Success, string Message)> RegisterUserAsync(RegisterationVM model, string webRootPath);
        Task<(bool Success, string Message)> UpdateUserAsync(EditUserVM model, string webRootPath);
        Task<(bool Success, string Message)> DeleteUserAsync(string id);
        Task<(bool Success, string Message)> ChangePasswordAsync(string email, ChangePasswordVM model);
        Task<(bool Success, string Message, string CallbackUrl)> ForgotPasswordAsync(ForgetPasswordVM model, string scheme, Func<string, string, object, string, string> urlAction);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordVM model);
        Task<(bool Success, string Message)> ToggleLockoutAsync(string id);
        Task<(bool Success, string Message, ApplicationUser User)> ValidateLoginAsync(LoginVM model);
    }
}