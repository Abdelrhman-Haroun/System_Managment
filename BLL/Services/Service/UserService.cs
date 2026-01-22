
using BLL.Services.IService;
using BLL.ViewModels.Account;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class UserService : IUserService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploader _fileUploader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            IFileUploader fileUploader,
            IEmailSender emailSender,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileUploader = fileUploader;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, IEnumerable<ApplicationUser> Users)> GetAllUsersAsync(string searchTerm)
        {
            try
            {
                var users = await _unitOfWork.User.GetAllAsync(u =>
                    string.IsNullOrWhiteSpace(searchTerm) ||
                    u.FullName.ToLower().Contains(searchTerm.ToLower()) ||
                    u.Email.ToLower().Contains(searchTerm.ToLower()) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                );

                return (true, null, users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return (false, "حدث خطأ أثناء جلب المستخدمين", null);
            }
        }

        public async Task<(bool Success, ApplicationUser User, string Role, DateTime? CreatedAt)> GetUserDetailsAsync(string id)
        {
            try
            {
                var user = await _unitOfWork.User.GetByIdAsync(id);
                if (user == null)
                    return (false, null, null, null);

                var roles = await _unitOfWork.User.GetUserRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "غير محدد";

                return (true, user, role, user.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details");
                return (false, null, null, null);
            }
        }

        public async Task<(bool Success, string Message, EditUserVM Model)> GetUserForEditAsync(string id)
        {
            try
            {
                var user = await _unitOfWork.User.GetByIdAsync(id);
                if (user == null)
                    return (false, "المستخدم غير موجود", null);

                var roles = await _unitOfWork.User.GetUserRolesAsync(user);
                var model = new EditUserVM
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.FirstOrDefault(),
                    ProfilePicture = user.ProfilePicture
                };

                return (true, null, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for edit");
                return (false, "حدث خطأ أثناء جلب بيانات المستخدم", null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterationVM model, string webRootPath)
        {
            try
            {
                if (await _unitOfWork.User.ExistsAsync(u => u.Email == model.Email))
                    return (false, "البريد الإلكتروني مستخدم من قبل");

                if (await _unitOfWork.User.ExistsAsync(u => u.FullName == model.FullName))
                    return (false, "الاسم الكامل مستخدم من قبل");

                string profilePicturePath = null;
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    try
                    {
                        profilePicturePath = await _fileUploader.UploadImageAsync(model.ProfilePicture, webRootPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Image upload error");
                        return (false, "فشل رفع الصورة، تأكد من نوع وحجم الملف");
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePicture = profilePicturePath,
                };

                var createdUser = await _unitOfWork.User.CreateAsync(user, model.Password);
                if (createdUser == null)
                    return (false, "فشل إنشاء الحساب");

                var roleResult = await _unitOfWork.User.AddToRoleAsync(createdUser, model.UserRole);
                if (!roleResult)
                {
                    await _unitOfWork.User.SoftDeleteAsync(createdUser.Id);
                    return (false, "فشل تعيين الدور، حاول مرة أخرى");
                }

                return (true, "تم إنشاء الحساب بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return (false, "حدث خطأ غير متوقع. يرجى المحاولة لاحقًا.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(EditUserVM model, string webRootPath)
        {
            try
            {
                var user = await _unitOfWork.User.GetByIdAsync(model.Id);
                if (user == null)
                    return (false, "المستخدم غير موجود");

                if (user.Email != model.Email)
                {
                    var existingUser = await _unitOfWork.User.GetByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != model.Id)
                        return (false, "البريد الإلكتروني مستخدم بالفعل من قبل مستخدم آخر");
                }

                if (user.FullName != model.FullName)
                {
                    var exists = await _unitOfWork.User.ExistsAsync(u => u.FullName == model.FullName && u.Id != model.Id);
                    if (exists)
                        return (false, "الاسم مستخدم بالفعل من قبل مستخدم آخر");
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.UpdateAt = DateTime.UtcNow;

                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    var newPath = await _fileUploader.UploadImageAsync(model.ProfilePictureFile, webRootPath);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        if (!string.IsNullOrEmpty(user.ProfilePicture))
                        {
                            try
                            {
                                var oldPath = Path.Combine(webRootPath, user.ProfilePicture.TrimStart('/'));
                                if (File.Exists(oldPath))
                                    File.Delete(oldPath);
                            }
                            catch { }
                        }
                        user.ProfilePicture = newPath;
                    }
                }

                var currentRoles = await _unitOfWork.User.GetUserRolesAsync(user);
                if (currentRoles.Any())
                    await _unitOfWork.User.RemoveFromRolesAsync(user, currentRoles);

                if (!string.IsNullOrEmpty(model.Role))
                    await _unitOfWork.User.AddToRoleAsync(user, model.Role);

                var result = await _unitOfWork.User.UpdateAsync(user);
                if (!result)
                    return (false, "فشل التحديث");

                return (true, "تم التحديث بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update error");
                return (false, "خطأ: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(string id)
        {
            try
            {
                var result = await _unitOfWork.User.SoftDeleteAsync(id);
                if (!result)
                    return (false, "المستخدم غير موجود");

                return (true, "تم حذف المستخدم بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete error");
                return (false, "حدث خطأ أثناء الحذف");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string email, ChangePasswordVM model)
        {
            try
            {
                var user = await _unitOfWork.User.GetByEmailAsync(email);
                if (user == null)
                    return (false, "المستخدم غير موجود");

                var result = await _unitOfWork.User.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result)
                    return (false, "فشل تغيير كلمة المرور");

                await _unitOfWork.User.UpdateSecurityStampAsync(user);
                return (true, "تم تغيير كلمة المرور بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change password error");
                return (false, "حدث خطأ أثناء تغيير كلمة المرور");
            }
        }

        public async Task<(bool Success, string Message, string CallbackUrl)> ForgotPasswordAsync(
            ForgetPasswordVM model,
            string scheme,
            Func<string, string, object, string, string> urlAction)
        {
            try
            {
                var user = await _unitOfWork.User.GetByEmailAsync(model.Email);
                if (user == null)
                    return (false, $"هذا المستخدم {model.Email} غير موجود", null);

                var code = await _unitOfWork.User.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = urlAction("ResetPassword", "Account", new { userId = user.Id, code }, scheme);
                var htmlMessage = $"Reset your AlSalem account password by clicking <a href='{callbackUrl}'>here</a>. Link valid for 3 hours.";

                await _emailSender.SendEmailAsync(model.Email, "AlSalem Password Reset", htmlMessage);
                return (true, "تم إرسال رابط إعادة تعيين كلمة المرور", callbackUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password error");
                return (false, "حدث خطأ أثناء إرسال البريد الإلكتروني", null);
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordVM model)
        {
            try
            {
                var user = await _unitOfWork.User.GetByEmailAsync(model.Email);
                if (user == null)
                    return (false, $"هذا المستخدم {model.Email} غير موجود");

                var result = await _unitOfWork.User.ResetPasswordAsync(user, model.Code, model.Password);
                if (!result)
                    return (false, "فشل إعادة تعيين كلمة المرور");

                await _unitOfWork.User.UpdateSecurityStampAsync(user);
                return (true, "تم إعادة تعيين كلمة المرور بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password error");
                return (false, "حدث خطأ أثناء إعادة تعيين كلمة المرور");
            }
        }

        public async Task<(bool Success, string Message)> ToggleLockoutAsync(string id)
        {
            try
            {
                var result = await _unitOfWork.User.ToggleLockoutAsync(id);
                if (!result)
                    return (false, "المستخدم غير موجود");

                return (true, "تم تغيير حالة القفل بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toggle lockout error");
                return (false, "حدث خطأ أثناء تغيير حالة القفل");
            }
        }

        public async Task<(bool Success, string Message, ApplicationUser User)> ValidateLoginAsync(LoginVM model)
        {
            try
            {
                var user = await _unitOfWork.User.GetByEmailAsync(model.Email);
                if (user == null)
                    return (false, $"هذا المستخدم {model.Email} غير موجود", null);

                if (await _unitOfWork.User.IsLockedOutAsync(user))
                    return (false, "الحساب مقفل", null);

                return (true, null, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login validation error");
                return (false, "حدث خطأ أثناء التحقق من بيانات الدخول", null);
            }
        }
    }
}
