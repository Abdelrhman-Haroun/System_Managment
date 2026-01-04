using BLL.Services.IService;
using BLL.ViewModels.Account;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace System_Managment.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region ctor
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IFileUploader _fileUploader;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AccountController> _logger;
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IFileUploader fileUploader, IWebHostEnvironment hostEnvironment, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _fileUploader = fileUploader;
            _hostingEnvironment = hostEnvironment;
            _logger = logger;
        }
        #endregion

        #region All Users
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, int page = 1)
        {
            var users = _userManager.Users.Where(u => !u.IsDeleted).AsQueryable();

            // Filter by search term if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                );
            }

            // Order by creation date
            var userList = await users.OrderByDescending(u => u.CreatedAt).ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;

            return View(userList);
        }
        #endregion

        #region Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", $"هذا المستخدم {model.Email} غير موجود.");
                return View(model);
            }

            // Check lockout status correctly
            if (await _userManager.IsLockedOutAsync(user))
            {
                return RedirectToAction("AccessDenied");
            }

            // Correct login call: use Username, not Email
            var result = await _signInManager.PasswordSignInAsync(user.UserName,model.Password,model.RememberMe,lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl ?? "/Home/Index");
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction("AccessDenied");
            }

            ModelState.AddModelError("Password", "كلمة المرور غير صحيحة");
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            // When requested via AJAX (e.g., for modal dialog)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers.Accept.Any(h => h?.Contains("application/json") == true))
            {
                return PartialView("_RegisterDialog", new RegisterationVM());
            }

            // Regular full page request
            return View(new RegisterationVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            try
            {
                if (await _userManager.Users.AnyAsync(u => u.Email == model.Email))
                {
                    return Json(new { success = false, errors = new { Email = new[] { "البريد الإلكتروني مستخدم من قبل" } } });
                }

                if (await _userManager.Users.AnyAsync(u => u.FullName == model.FullName))
                {
                    return Json(new { success = false, errors = new { FullName = new[] { "الاسم الكامل مستخدم من قبل" } } });
                }

                string profilePicturePath = null;
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    try
                    {
                        profilePicturePath = await _fileUploader.UploadImageAsync(
                            model.ProfilePicture,
                            _hostingEnvironment.WebRootPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Image upload error: {ex.Message}");
                        return Json(new { success = false, errors = new { ProfilePicture = new[] { "فشل رفع الصورة، تأكد من نوع وحجم الملف" } } });
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePicture = profilePicturePath,
                    CreatedAt = DateTime.UtcNow,
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);

                if (!createResult.Succeeded)
                {
                    var identityErrors = createResult.Errors
                        .GroupBy(e => e.Code.Contains("Password") ? "Password" :
                                      e.Code.Contains("Email") ? "Email" :
                                      e.Code.Contains("User") ? "Email" : "General")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Description).ToArray()
                        );

                    return Json(new { success = false, errors = identityErrors });
                }

                var roleResult = await _userManager.AddToRoleAsync(user, model.UserRole);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return Json(new
                    {
                        success = false,
                        errors = new { UserRole = new[] { "فشل تعيين الدور، حاول مرة أخرى" } }
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "تم إنشاء الحساب بنجاح"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return Json(new
                {
                    success = false,
                    message = "حدث خطأ غير متوقع. يرجى المحاولة لاحقًا."
                });
            }
        }

        #endregion

        #region Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        #endregion

        #region Forgot Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "هذا المستخدم " + model.Email + " غير موجود");
                    return View(model);
                }
                
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, Request.Scheme);
                var htmlMessage = $"Reset your AlSalem account password by clicking <a href='{callbackUrl}'>here</a>. Link valid for 3 hours.";

                await _emailSender.SendEmailAsync(model.Email, "AlSalem Password Reset", htmlMessage);
                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Reset Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId = null, string code = null)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Invalid password reset link.");
            }
            var model = new ResetPasswordVM { UserId = userId, Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "هذا المستخدم " + model.Email + " غير موجود");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // جيب الـ Roles هنا في الـ Controller (مش في الـ View)
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "غير محدد";

            // جيب تاريخ الإنشاء لو موجود
            ViewBag.CreatedAt = user.CreatedAt; // أو أي property عندك

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DetailsPartial", user);

            return View(user);
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault(), 
                ProfilePicture = user.ProfilePicture
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EditPartial", model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM model)
        {
            try
            {
                // Remove ProfilePicture fields from validation
                ModelState.Remove("ProfilePicture");
                ModelState.Remove("ProfilePictureFile");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", errors) });
                }

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return Json(new { success = false, message = "المستخدم غير موجود" });

                // Check if Email is already taken by another user
                if (user.Email != model.Email)
                {
                    var existingUserWithEmail = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUserWithEmail != null && existingUserWithEmail.Id != model.Id)
                    {
                        return Json(new { success = false, message = "البريد الإلكتروني مستخدم بالفعل من قبل مستخدم آخر" });
                    }
                }

                // Check if FullName is already taken by another user
                if (user.FullName != model.FullName)
                {
                    var existingUserWithName = _userManager.Users
                        .FirstOrDefault(u => u.FullName == model.FullName && u.Id != model.Id);

                    if (existingUserWithName != null)
                    {
                        return Json(new { success = false, message = "الاسم مستخدم بالفعل من قبل مستخدم آخر" });
                    }
                }

                // Update basic info
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                // Profile picture - only if new file uploaded
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    string wwwRootPath = _hostingEnvironment.WebRootPath;
                    var newPath = await _fileUploader.UploadImageAsync(model.ProfilePictureFile, wwwRootPath);

                    if (!string.IsNullOrEmpty(newPath))
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(user.ProfilePicture))
                        {
                            try
                            {
                                var oldPath = Path.Combine(wwwRootPath, user.ProfilePicture.TrimStart('/'));
                                if (System.IO.File.Exists(oldPath))
                                    System.IO.File.Delete(oldPath);
                            }
                            catch { }
                        }
                        user.ProfilePicture = newPath;
                    }
                }

                // Update role
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!string.IsNullOrEmpty(model.Role))
                    await _userManager.AddToRoleAsync(user, model.Role);


                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = "فشل التحديث: " + errors });
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ: " + ex.Message });
            }
        }
        #endregion

        #region Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Json(new
                {
                    success = false,
                    errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            return RedirectToAction("Index");
        }
        #endregion

        #region Change Password
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(User.Identity.Name);

                if (user == null)
                    return NotFound();

                // Use the current password from the model, not the hash
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    return RedirectToAction("Index");
                }

                // Add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        #endregion

        #region Access Denied 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        #endregion

        #region Lockout
        [HttpPost]
        public async Task<IActionResult> ToggleLockout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                user.LockoutEnd = null;
            else
                user.LockoutEnd = DateTime.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        #endregion
    }

}

