//using BLL.Services.IService;
//using BLL.ViewModels.Account;
//using DAL.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace System_Managment.Controllers
//{
//    public class SupplierController : Controller
//    {
//        #region ctor

//        private readonly IFileUploader _fileUploader;
//        private readonly IWebHostEnvironment _hostingEnvironment;
//        private readonly ILogger<SupplierController> _logger;
//        public SupplierController(IFileUploader fileUploader, IWebHostEnvironment hostEnvironment, ILogger<SupplierController> logger)
//        {
//            _fileUploader = fileUploader;
//            _hostingEnvironment = hostEnvironment;
//            _logger = logger;
//        }
//        #endregion

//        #region All Users

//        [HttpGet]
//        public async Task<IActionResult> Index(string searchTerm)
//        {
//            var users = _userManager.Users.AsQueryable();

//            // Filter by search term if provided
//            if (!string.IsNullOrWhiteSpace(searchTerm))
//            {
//                searchTerm = searchTerm.Trim().ToLower();
//                users = users.Where(u =>
//                    u.FullName.ToLower().Contains(searchTerm) ||
//                    u.Email.ToLower().Contains(searchTerm) ||
//                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
//                );
//            }

//            // Order by creation date
//            var userList = await users.OrderBy(u => u.CreatedAt).ToListAsync();

//            ViewBag.SearchTerm = searchTerm;

//            return View(userList);
//        }
//        #endregion

//        #region Create
//        [HttpGet]
//        public IActionResult Create()
//        {
//            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//            {
//                // Return the partial view for AJAX request
//                return PartialView("Register", new RegisterationVM());
//            }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(RegisterationVM model)
//        {
//            string wwwRootPath = _hostingEnvironment.WebRootPath;

//            if (ModelState.IsValid)
//            {
//                var username = _userManager.Users.FirstOrDefault(u => u.UserName == model.Email);
//                var fullName = _userManager.Users.FirstOrDefault(u => u.FullName == model.FullName);
//                if (username != null)
//                {
//                    ModelState.AddModelError("Email", "هذا الايميل " + model.Email + " مستخدم من قبل");
//                }
//                else if (fullName != null)
//                {
//                    ModelState.AddModelError("FullName", "هذا الاسم " + model.FullName + " مستخدم من قبل");
//                }
//                else
//                {
//                    var user = new ApplicationUser
//                    {
//                        UserName = model.Email,
//                        Email = model.Email,
//                        FullName = model.FullName,
//                        PhoneNumber = model.PhoneNumber,
//                        ProfilePicture = await _fileUploader.UploadImageAsync(model.ProfilePicture, wwwRootPath)
//                    };

//                    var result = await _userManager.CreateAsync(user, model.Password);
//                    if (result.Succeeded)
//                    {
//                        await _userManager.AddToRoleAsync(user, model.UserRole);

//                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                            return Json(new { success = true });

//                        return RedirectToAction("Index");
//                    }

//                    foreach (var error in result.Errors)
//                    {
//                        ModelState.AddModelError("", error.Description);
//                    }
//                }
//            }

//            // If AJAX, return the partial with validation errors
//            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                return PartialView("Register", model);

//            return View(model);
//        }

//        #endregion

//        #region Details
//        [HttpGet]
//        public IActionResult Details(string id)
//        {
//            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
//            if (user == null)
//                return NotFound();

//            return PartialView(user);
//        }

//        #endregion

//        #region Edit

//        [HttpGet]
//        public async Task<IActionResult> Edit(string id)
//        {
//            if (string.IsNullOrEmpty(id))
//                return NotFound();

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null)
//                return NotFound();

//            var roles = await _userManager.GetRolesAsync(user);
//            var model = new EditUserVM
//            {
//                Id = user.Id,
//                FullName = user.FullName,
//                Email = user.Email,
//                PhoneNumber = user.PhoneNumber,
//                Role = roles.FirstOrDefault(),
//                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow,
//                ProfilePicture = user.ProfilePicture
//            };

//            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                return PartialView("Edit", model);

//            return View(model);
//        }
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(EditUserVM model)
//        {
//            try
//            {
//                // Remove ProfilePicture fields from validation
//                ModelState.Remove("ProfilePicture");
//                ModelState.Remove("ProfilePictureFile");

//                if (!ModelState.IsValid)
//                {
//                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                    return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", errors) });
//                }

//                var user = await _userManager.FindByIdAsync(model.Id);
//                if (user == null)
//                    return Json(new { success = false, message = "المستخدم غير موجود" });

//                // Check if Email is already taken by another user
//                if (user.Email != model.Email)
//                {
//                    var existingUserWithEmail = await _userManager.FindByEmailAsync(model.Email);
//                    if (existingUserWithEmail != null && existingUserWithEmail.Id != model.Id)
//                    {
//                        return Json(new { success = false, message = "البريد الإلكتروني مستخدم بالفعل من قبل مستخدم آخر" });
//                    }
//                }

//                // Check if FullName is already taken by another user
//                if (user.FullName != model.FullName)
//                {
//                    var existingUserWithName = _userManager.Users
//                        .FirstOrDefault(u => u.FullName == model.FullName && u.Id != model.Id);

//                    if (existingUserWithName != null)
//                    {
//                        return Json(new { success = false, message = "الاسم مستخدم بالفعل من قبل مستخدم آخر" });
//                    }
//                }

//                // Update basic info
//                user.FullName = model.FullName;
//                user.Email = model.Email;
//                user.UserName = model.Email;
//                user.PhoneNumber = model.PhoneNumber;

//                // Profile picture - only if new file uploaded
//                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
//                {
//                    string wwwRootPath = _hostingEnvironment.WebRootPath;
//                    var newPath = await _fileUploader.UploadImageAsync(model.ProfilePictureFile, wwwRootPath);

//                    if (!string.IsNullOrEmpty(newPath))
//                    {
//                        // Delete old file if exists
//                        if (!string.IsNullOrEmpty(user.ProfilePicture))
//                        {
//                            try
//                            {
//                                var oldPath = Path.Combine(wwwRootPath, user.ProfilePicture.TrimStart('/'));
//                                if (System.IO.File.Exists(oldPath))
//                                    System.IO.File.Delete(oldPath);
//                            }
//                            catch { }
//                        }
//                        user.ProfilePicture = newPath;
//                    }
//                }

//                // Update role
//                var currentRoles = await _userManager.GetRolesAsync(user);
//                if (currentRoles.Any())
//                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

//                if (!string.IsNullOrEmpty(model.Role))
//                    await _userManager.AddToRoleAsync(user, model.Role);

//                // Lock status
//                user.LockoutEnd = model.IsLocked ? DateTime.UtcNow.AddYears(100) : null;

//                var result = await _userManager.UpdateAsync(user);

//                if (!result.Succeeded)
//                {
//                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//                    return Json(new { success = false, message = "فشل التحديث: " + errors });
//                }

//                return Json(new { success = true });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, message = "خطأ: " + ex.Message });
//            }
//        }
//        #endregion

//        #region Delete
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete([FromForm] string id)
//        {
//            if (string.IsNullOrEmpty(id))
//                return Json(new { success = false, message = "معرف المستخدم غير صحيح" });

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null)
//                return Json(new { success = false, message = "المستخدم غير موجود" });

//            var result = await _userManager.DeleteAsync(user);
//            if (!result.Succeeded)
//            {
//                return Json(new
//                {
//                    success = false,
//                    errors = result.Errors.Select(e => e.Description).ToList()
//                });
//            }

//            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                return Json(new { success = true });

//            return RedirectToAction("Index");
//        }
//        #endregion
//    }
//}
