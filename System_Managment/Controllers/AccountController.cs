using BLL.Services.IService;
using BLL.ViewModels.Account;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            IUserService userService,
            IWebHostEnvironment hostEnvironment)
        {
            _signInManager = signInManager;
            _userService = userService;
            _hostingEnvironment = hostEnvironment;
        }

        #region All Users
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, int page = 1)
        {
            var result = await _userService.GetAllUsersAsync(searchTerm);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ApplicationUser>());
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;

            return View(result.Users);
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

            var validation = await _userService.ValidateLoginAsync(model);

            if (!validation.Success)
            {
                if (validation.Message.Contains("مقفل"))
                    return RedirectToAction("AccessDenied");

                ModelState.AddModelError("Email", validation.Message);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                validation.User.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl ?? "/Home/Index");

            if (result.IsLockedOut)
                return RedirectToAction("AccessDenied");

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
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers.Accept.Any(h => h?.Contains("application/json") == true))
            {
                return PartialView("_RegisterDialog", new RegisterationVM());
            }

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

            var result = await _userService.RegisterUserAsync(model, _hostingEnvironment.WebRootPath);

            if (!result.Success)
            {
                return Json(new
                {
                    success = false,
                    errors = new { General = new[] { result.Message } }
                });
            }

            return Json(new
            {
                success = true,
                message = result.Message
            });
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
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.ForgotPasswordAsync(
                model,
                Request.Scheme,
                Url.Action);

            if (!result.Success)
            {
                ModelState.AddModelError("Email", result.Message);
                return View(model);
            }

            return View("ForgotPasswordConfirmation");
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
                return BadRequest("Invalid password reset link.");

            var model = new ResetPasswordVM { UserId = userId, Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.ResetPasswordAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError("Email", result.Message);
                return View(model);
            }

            return RedirectToAction("ResetPasswordConfirmation");
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

            var result = await _userService.GetUserDetailsAsync(id);

            if (!result.Success)
                return NotFound();

            ViewBag.UserRole = result.Role;
            ViewBag.CreatedAt = result.CreatedAt;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DetailsPartial", result.User);

            return View(result.User);
        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var result = await _userService.GetUserForEditAsync(id);

            if (!result.Success)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EditPartial", result.Model);

            return View(result.Model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM model)
        {
            ModelState.Remove("ProfilePicture");
            ModelState.Remove("ProfilePictureFile");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", errors) });
            }

            var result = await _userService.UpdateUserAsync(model, _hostingEnvironment.WebRootPath);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        #endregion

        #region Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });

            var result = await _userService.DeleteUserAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = result.Success, message = result.Message });

            if (result.Success)
                return RedirectToAction("Index");

            TempData["Error"] = result.Message;
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
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.ChangePasswordAsync(User.Identity.Name, model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            return RedirectToAction("Index");
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
            var result = await _userService.ToggleLockoutAsync(id);

            if (!result.Success)
                return NotFound();

            return RedirectToAction("Index");
        }
        #endregion
    }
}