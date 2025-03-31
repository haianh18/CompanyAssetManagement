using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminController(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsers = await _unitOfWork.Users.CountUsersAsync(),
                    ActiveUsers = (await _unitOfWork.Users.GetActiveUsersAsync()).Count(),
                    InactiveUsers = (await _unitOfWork.Users.GetInactiveUsersAsync()).Count(),
                    TotalWarehouses = (await _unitOfWork.Warehouses.GetAllAsync()).Count(),
                    TotalAssets = (await _unitOfWork.Assets.GetAllAsync()).Count(),
                    TotalCategories = (await _unitOfWork.AssetCategories.GetAllAsync()).Count(),
                    TotalDepartments = (await _unitOfWork.Departments.GetAllAsync()).Count(),
                    RecentUsers = (await _unitOfWork.Users.GetAllUsersAsync()).OrderByDescending(u => u.DateCreated).Take(5).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
                return View(new AdminDashboardViewModel());
            }
        }

        #region User Management

        // GET: Admin/Users
        public async Task<IActionResult> Users(string searchString, bool showInactive = false, int page = 1)
        {
            try
            {
                // Get all users
                var allUsers = await _unitOfWork.Users.GetAllUsersAsync();
                var allUsersIncludeDeleted = await _unitOfWork.Users.GetAllIncludingDeleted();
                // Filter by active/inactive status if needed
                var filteredUsers = showInactive
                    ? allUsers
                    : allUsersIncludeDeleted;

                // Apply search if provided
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    filteredUsers = filteredUsers.Where(u =>
                        u.UserName.ToLower().Contains(searchString) ||
                        u.FullName.ToLower().Contains(searchString) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchString))
                    );
                }

                // Order users
                var orderedUsers = filteredUsers.OrderBy(u => u.UserName).ToList();

                // Pagination
                int pageSize = 10;
                int totalItems = orderedUsers.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages));

                var pagedUsers = orderedUsers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Get all roles and departments for filtering/display
                var roles = await _unitOfWork.Roles.GetAllRolesAsync();
                var departments = await _unitOfWork.Departments.GetAllAsync();

                var viewModel = new UserManagementViewModel
                {
                    Users = pagedUsers,
                    Roles = roles.ToList(),
                    Departments = departments.ToList(),
                    ShowInactive = showInactive,
                    SearchString = searchString,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu người dùng: {ex.Message}";
                return View(new UserManagementViewModel());
            }
        }

        // GET: Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _unitOfWork.Roles.GetAllRolesAsync();
            var departments = await _unitOfWork.Departments.GetAllAsync();

            var viewModel = new UserCreateViewModel
            {
                AvailableRoles = roles.ToList(),
                AvailableDepartments = departments.ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            // Loại bỏ validation cho Role
            ModelState.Remove("User.Role");

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra username tồn tại
                    var existingUser = await _userManager.FindByNameAsync(model.User.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("User.UserName", "Tên đăng nhập này đã tồn tại.");
                        model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
                        model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();
                        return View(model);
                    }

                    // Thiết lập thông tin user
                    var user = model.User;
                    user.RoleId = model.SelectedRoleId;
                    user.DepartmentId = model.SelectedDepartmentId;
                    user.DateCreated = DateTime.Now;
                    user.IsDeleted = false;

                    // Tạo user với mật khẩu
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // Lấy role dựa trên ID
                        var role = await _roleManager.FindByIdAsync(model.SelectedRoleId.ToString());
                        if (role != null)
                        {
                            // Thêm user vào role
                            await _userManager.AddToRoleAsync(user, role.Name);
                        }

                        TempData["SuccessMessage"] = $"Người dùng '{user.UserName}' đã được tạo thành công.";
                        return RedirectToAction(nameof(Users));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi tạo người dùng: {ex.Message}");
                }
            }

            // Nếu có lỗi, tải lại dữ liệu và hiển thị lại form
            model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
            model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

            return View(model);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Users));
            }

            var isAdmin = await _unitOfWork.Roles.GetRoleByIdAsync(user.RoleId);
            if (isAdmin != null && isAdmin.Name == "Admin")
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa tài khoản Admin.";
                return RedirectToAction(nameof(Users));
            }

            var roles = await _unitOfWork.Roles.GetAllRolesAsync();
            var departments = await _unitOfWork.Departments.GetAllAsync();

            var viewModel = new UserEditViewModel
            {
                User = user,
                SelectedRoleId = user.RoleId,
                SelectedDepartmentId = user.DepartmentId,
                AvailableRoles = roles.ToList(),
                AvailableDepartments = departments.ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserEditViewModel model)
        {
            // Validate ID match
            if (id != model.User.Id)
            {
                TempData["ErrorMessage"] = "ID người dùng không khớp.";
                return RedirectToAction(nameof(Users));
            }

            // Custom validation for password reset
            if (model.ResetPassword && !model.ValidatePasswordReset())
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ và chính xác mật khẩu mới.");
            }

            // Remove validation for password if not resetting
            if (!model.ResetPassword)
            {
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmPassword");
            }
            // Loại bỏ validation cho Role
            ModelState.Remove("User.Role");

            // Validate model state
            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing user from database
                    var user = await _unitOfWork.Users.GetUserByIdAsync(id);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                        return RedirectToAction(nameof(Users));
                    }

                    // Prevent editing admin account
                    var isAdmin = await _unitOfWork.Roles.GetRoleByIdAsync(user.RoleId);
                    if (isAdmin != null && isAdmin.Name == "Admin")
                    {
                        TempData["ErrorMessage"] = "Không thể chỉnh sửa tài khoản Admin.";
                        return RedirectToAction(nameof(Users));
                    }

                    // Check for duplicate username
                    if (user.UserName != model.User.UserName)
                    {
                        var existingUser = await _userManager.FindByNameAsync(model.User.UserName);
                        if (existingUser != null && existingUser.Id != id)
                        {
                            ModelState.AddModelError("User.UserName", "Tên đăng nhập này đã tồn tại.");

                            // Reload roles and departments
                            model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
                            model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

                            return View(model);
                        }
                    }

                    // Check for duplicate email
                    if (!string.IsNullOrEmpty(model.User.Email) && user.Email != model.User.Email)
                    {
                        var existingEmail = await _userManager.FindByEmailAsync(model.User.Email);
                        if (existingEmail != null && existingEmail.Id != id)
                        {
                            ModelState.AddModelError("User.Email", "Email này đã được sử dụng.");

                            // Reload roles and departments
                            model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
                            model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

                            return View(model);
                        }
                    }

                    // Update user properties
                    user.UserName = model.User.UserName;
                    user.FullName = model.User.FullName;
                    user.Email = model.User.Email;
                    user.PhoneNumber = model.User.PhoneNumber;
                    user.BirthDay = model.User.BirthDay;
                    user.Specification = model.User.Specification;
                    user.DateModified = DateTime.Now;

                    // Handle role change
                    if (user.RoleId != model.SelectedRoleId)
                    {
                        // Get current role
                        var currentRole = await _roleManager.FindByIdAsync(user.RoleId.ToString());
                        if (currentRole != null)
                        {
                            // Remove from current role
                            await _userManager.RemoveFromRoleAsync(user, currentRole.Name);
                        }

                        // Get new role
                        var newRole = await _roleManager.FindByIdAsync(model.SelectedRoleId.ToString());
                        if (newRole != null)
                        {
                            // Add to new role
                            await _userManager.AddToRoleAsync(user, newRole.Name);
                        }

                        // Update RoleId
                        user.RoleId = model.SelectedRoleId;
                    }

                    // Update department
                    user.DepartmentId = model.SelectedDepartmentId;

                    // Update user
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        // Reload roles and departments
                        model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
                        model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

                        return View(model);
                    }

                    // Handle password reset if requested
                    if (model.ResetPassword)
                    {
                        // Remove existing password
                        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                        if (!removePasswordResult.Succeeded)
                        {
                            foreach (var error in removePasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, $"Lỗi khi xóa mật khẩu cũ: {error.Description}");
                            }

                            // Reload roles and departments
                            model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
                            model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

                            return View(model);
                        }

                        // Add new password
                        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        if (!addPasswordResult.Succeeded)
                        {
                            foreach (var error in addPasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, $"Lỗi khi thêm mật khẩu mới: {error.Description}");
                            }

                            // Reload roles and departments
                            model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
                            model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

                            return View(model);
                        }
                    }

                    TempData["SuccessMessage"] = $"Người dùng '{user.UserName}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Users));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật người dùng: {ex.Message}";
                }
            }

            // If we got this far, something failed, reload data and redisplay form
            model.AvailableRoles = (await _unitOfWork.Roles.GetAllRolesAsync()).ToList();
            model.AvailableDepartments = (await _unitOfWork.Departments.GetAllAsync()).ToList();

            return View(model);
        }

        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng trong hệ thống.";
                    return RedirectToAction(nameof(Users));
                }

                // Prevent deleting the current admin user
                if (User.Identity.Name == user.UserName)
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài khoản hiện đang đăng nhập.";
                    return RedirectToAction(nameof(Users));
                }

                // Check if it's an Admin account
                var isAdmin = await _unitOfWork.Roles.GetRoleByIdAsync(user.RoleId);
                if (isAdmin != null && isAdmin.Name == "Admin")
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài khoản Admin.";
                    return RedirectToAction(nameof(Users));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction(nameof(Users));
            }
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetUserByIdAsync(id);
                Console.WriteLine($"User found: {user != null}, UserName: {user?.UserName}");
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng trong hệ thống.";
                    return RedirectToAction(nameof(Users));
                }

                // Phòng tránh xóa Admin
                if (User.Identity.Name == user.UserName)
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài khoản hiện đang đăng nhập.";
                    return RedirectToAction(nameof(Users));
                }

                // Tiến hành soft delete
                user.IsDeleted = true;
                user.DateModified = DateTime.Now;
                user.DeletedDate = DateTime.Now;

                var updateResult = await _unitOfWork.Users.UpdateUserAsync(user);

                if (updateResult)
                {
                    // Xử lý các tham chiếu đến người dùng trong các ticket
                    //await _unitOfWork.Users.HandleDeletedUserInTicketsAsync(id);

                    // Chủ động lưu thay đổi
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Người dùng '{user.UserName}' đã được xóa thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái người dùng.";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteUserConfirmed: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi xóa người dùng: {ex.Message}";
                return RedirectToAction(nameof(Users));
            }
        }

        // GET: Admin/RestoreUser/5
        public async Task<IActionResult> RestoreUser(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Users), new { showInactive = true });
            }

            if (!user.IsDeleted)
            {
                TempData["WarningMessage"] = "Người dùng này chưa bị xóa.";
                return RedirectToAction(nameof(Users));
            }

            return View(user);
        }

        // POST: Admin/RestoreUser/5
        [HttpPost, ActionName("RestoreUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUserConfirmed(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Users), new { showInactive = true });
            }

            if (!user.IsDeleted)
            {
                TempData["WarningMessage"] = "Người dùng này chưa bị xóa.";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                // Restore the user
                user.IsDeleted = false;
                user.DeletedDate = null;
                user.DateModified = DateTime.Now;

                await _unitOfWork.Users.UpdateUserAsync(user);

                TempData["SuccessMessage"] = $"Người dùng '{user.UserName}' đã được khôi phục thành công.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi khôi phục người dùng: {ex.Message}";
                return View(user);
            }
        }

        #endregion

        #region Department Management

        // GET: Admin/Departments
        public async Task<IActionResult> Departments(string searchString, int page = 1)
        {
            try
            {
                // Get all departments
                IEnumerable<Department> departments;
                departments = await _unitOfWork.Departments.GetAllIncludingDeletedAsync();


                // Apply search if provided
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    departments = departments.Where(d =>
                        d.Name.ToLower().Contains(searchString) ||
                        (d.Email != null && d.Email.ToLower().Contains(searchString)) ||
                        (d.Phone != null && d.Phone.ToLower().Contains(searchString))
                    );
                }

                // Order departments
                var orderedDepartments = departments.OrderBy(d => d.Name).ToList();

                // Pagination
                int pageSize = 10;
                int totalItems = orderedDepartments.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages));

                var pagedDepartments = orderedDepartments
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new DepartmentManagementViewModel
                {
                    Departments = pagedDepartments,
                    ShowInactive = true,
                    SearchString = searchString,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu phòng ban: {ex.Message}";
                return View(new DepartmentManagementViewModel());
            }
        }

        // GET: Admin/CreateDepartment
        public IActionResult CreateDepartment()
        {
            return View(new Department());
        }

        // POST: Admin/CreateDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a department with the same name already exists
                    var existingDepartments = await _unitOfWork.Departments.GetAllAsync();
                    if (existingDepartments.Any(d => d.Name.Equals(department.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại phòng ban với tên này. Vui lòng chọn tên khác.");
                        return View(department);
                    }

                    // Set up timestamps
                    department.DateCreated = DateTime.Now;
                    department.IsDeleted = false;

                    // Add the department
                    await _unitOfWork.Departments.AddAsync(department);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Phòng ban '{department.Name}' đã được tạo thành công.";
                    return RedirectToAction(nameof(Departments));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Tạo phòng ban thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo phòng ban.");
                }
            }

            return View(department);
        }

        // GET: Admin/EditDepartment/5
        public async Task<IActionResult> EditDepartment(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng ban.";
                return RedirectToAction(nameof(Departments));
            }

            return View(department);
        }

        // POST: Admin/EditDepartment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, Department department)
        {
            if (id != department.Id)
            {
                TempData["ErrorMessage"] = "ID phòng ban không khớp.";
                return RedirectToAction(nameof(Departments));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing department
                    var existingDepartment = await _unitOfWork.Departments.GetByIdAsync(id);
                    if (existingDepartment == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy phòng ban.";
                        return RedirectToAction(nameof(Departments));
                    }

                    // Check for duplicate names
                    var allDepartments = await _unitOfWork.Departments.GetAllAsync();
                    if (allDepartments.Any(d => d.Id != id && d.Name.Equals(department.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại phòng ban khác với tên này. Vui lòng chọn tên khác.");
                        return View(department);
                    }

                    // Update properties
                    existingDepartment.Name = department.Name;
                    existingDepartment.Email = department.Email;
                    existingDepartment.Phone = department.Phone;
                    existingDepartment.DateModified = DateTime.Now;

                    // Update department
                    _unitOfWork.Departments.Update(existingDepartment);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Phòng ban '{existingDepartment.Name}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Departments));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật phòng ban thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật phòng ban.");
                }
            }

            return View(department);
        }

        // GET: Admin/DeleteDepartment/5
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng ban.";
                return RedirectToAction(nameof(Departments));
            }

            // Check if this is "Unassigned" department which shouldn't be deleted
            if (department.Name == "Unassigned")
            {
                TempData["ErrorMessage"] = $"Phòng ban '{department.Name}' không thể xóa vì đây là phòng ban mặc định.";
                return RedirectToAction(nameof(Departments));
            }

            return View(department);
        }

        // POST: Admin/DeleteDepartment/5
        [HttpPost, ActionName("DeleteDepartment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartmentConfirmed(int id)
        {
            try
            {
                // Get department
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy phòng ban.";
                    return RedirectToAction(nameof(Departments));
                }

                // Check if this is "Unassigned" department which shouldn't be deleted
                if (department.Name == "Unassigned")
                {
                    TempData["ErrorMessage"] = $"Phòng ban '{department.Name}' không thể xóa vì đây là phòng ban mặc định.";
                    return RedirectToAction(nameof(Departments));
                }

                // Soft delete department and handle department users
                await _unitOfWork.Departments.SoftDeleteDepartmentAsync(id);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Phòng ban '{department.Name}' đã được xóa thành công.";
                return RedirectToAction(nameof(Departments));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xóa phòng ban thất bại: {ex.Message}";
                return RedirectToAction(nameof(Departments));
            }
        }

        // GET: Admin/RestoreDepartment/5
        public async Task<IActionResult> RestoreDepartment(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdIncludingDeletedAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng ban.";
                return RedirectToAction(nameof(Departments), new { showInactive = true });
            }

            if (!department.IsDeleted)
            {
                TempData["WarningMessage"] = "Phòng ban này chưa bị xóa.";
                return RedirectToAction(nameof(Departments));
            }

            return View(department);
        }

        // POST: Admin/RestoreDepartment/5
        [HttpPost, ActionName("RestoreDepartment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreDepartmentConfirmed(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdIncludingDeletedAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng ban.";
                return RedirectToAction(nameof(Departments), new { showInactive = true });
            }

            if (!department.IsDeleted)
            {
                TempData["WarningMessage"] = "Phòng ban này chưa bị xóa.";
                return RedirectToAction(nameof(Departments));
            }

            try
            {
                // Restore department
                await _unitOfWork.Departments.RestoreDeletedAsync(id);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Phòng ban '{department.Name}' đã được khôi phục thành công.";
                return RedirectToAction(nameof(Departments));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Khôi phục phòng ban thất bại: {ex.Message}";
                return RedirectToAction(nameof(Departments), new { showInactive = true });
            }
        }

        #endregion

        #region Warehouse Management

        // GET: Admin/Warehouses
        public async Task<IActionResult> Warehouses(string searchString, bool showInactive = false, int page = 1)
        {
            try
            {
                IEnumerable<Warehouse> warehouses;
                warehouses = await _unitOfWork.Warehouses.GetAllIncludingDeletedAsync();

                // Apply search if provided
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    warehouses = warehouses.Where(w =>
                        w.Name.ToLower().Contains(searchString) ||
                        (w.Address != null && w.Address.ToLower().Contains(searchString))
                    );
                }

                // Order warehouses
                var orderedWarehouses = warehouses.OrderBy(w => w.Name).ToList();

                // Pagination
                int pageSize = 10;
                int totalItems = orderedWarehouses.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages));

                var pagedWarehouses = orderedWarehouses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new WarehouseManagementViewModel
                {
                    Warehouses = pagedWarehouses,
                    ShowInactive = showInactive,
                    SearchString = searchString,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu kho: {ex.Message}";
                return View(new WarehouseManagementViewModel());
            }
        }

        // GET: Admin/CreateWarehouse
        public IActionResult CreateWarehouse()
        {
            return View(new Warehouse());
        }

        // POST: Admin/CreateWarehouse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWarehouse(Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a warehouse with the same name already exists
                    var existingWarehouses = await _unitOfWork.Warehouses.GetAllAsync();
                    if (existingWarehouses.Any(w => w.Name.Equals(warehouse.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại kho với tên này. Vui lòng chọn tên khác.");
                        return View(warehouse);
                    }

                    // Set up timestamps
                    warehouse.DateCreated = DateTime.Now;
                    warehouse.IsDeleted = false;

                    // Add the warehouse
                    await _unitOfWork.Warehouses.AddAsync(warehouse);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Kho '{warehouse.Name}' đã được tạo thành công.";
                    return RedirectToAction(nameof(Warehouses));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Tạo kho thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo kho.");
                }
            }

            return View(warehouse);
        }

        // GET: Admin/EditWarehouse/5
        public async Task<IActionResult> EditWarehouse(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kho.";
                return RedirectToAction(nameof(Warehouses));
            }

            return View(warehouse);
        }

        // POST: Admin/EditWarehouse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWarehouse(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                TempData["ErrorMessage"] = "ID kho không khớp.";
                return RedirectToAction(nameof(Warehouses));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing warehouse
                    var existingWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
                    if (existingWarehouse == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy kho.";
                        return RedirectToAction(nameof(Warehouses));
                    }

                    // Check for duplicate names
                    var allWarehouses = await _unitOfWork.Warehouses.GetAllAsync();
                    if (allWarehouses.Any(w => w.Id != id && w.Name.Equals(warehouse.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại kho khác với tên này. Vui lòng chọn tên khác.");
                        return View(warehouse);
                    }

                    // Update properties
                    existingWarehouse.Name = warehouse.Name;
                    existingWarehouse.Address = warehouse.Address;
                    existingWarehouse.DateModified = DateTime.Now;

                    // Update warehouse
                    _unitOfWork.Warehouses.Update(existingWarehouse);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Kho '{existingWarehouse.Name}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Warehouses));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật kho thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật kho.");
                }
            }

            return View(warehouse);
        }

        // GET: Admin/DeleteWarehouse/5
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kho.";
                return RedirectToAction(nameof(Warehouses));
            }

            // Check if this is "Unassigned Storage" or "Main Warehouse" which shouldn't be deleted
            if (warehouse.Name == "Unassigned Storage" || warehouse.Name == "Main Warehouse")
            {
                TempData["ErrorMessage"] = $"Kho '{warehouse.Name}' không thể xóa vì đây là kho hệ thống.";
                return RedirectToAction(nameof(Warehouses));
            }

            return View(warehouse);
        }

        // POST: Admin/DeleteWarehouse/5
        [HttpPost, ActionName("DeleteWarehouse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWarehouseConfirmed(int id)
        {
            try
            {
                // Get warehouse
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
                if (warehouse == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy kho.";
                    return RedirectToAction(nameof(Warehouses));
                }

                // Check if this is "Unassigned Storage" or "Main Warehouse" which shouldn't be deleted
                if (warehouse.Name == "Unassigned Storage" || warehouse.Name == "Main Warehouse")
                {
                    TempData["ErrorMessage"] = $"Kho '{warehouse.Name}' không thể xóa vì đây là kho hệ thống.";
                    return RedirectToAction(nameof(Warehouses));
                }

                // Soft delete warehouse and handle warehouse assets
                await _unitOfWork.Warehouses.SoftDeleteWarehouseAsync(id);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Kho '{warehouse.Name}' đã được xóa thành công.";
                return RedirectToAction(nameof(Warehouses));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xóa kho thất bại: {ex.Message}";
                return RedirectToAction(nameof(Warehouses));
            }
        }

        // GET: Admin/RestoreWarehouse/5
        public async Task<IActionResult> RestoreWarehouse(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdIncludingDeletedAsync(id);
            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kho.";
                return RedirectToAction(nameof(Warehouses), new { showInactive = true });
            }

            if (!warehouse.IsDeleted)
            {
                TempData["WarningMessage"] = "Kho này chưa bị xóa.";
                return RedirectToAction(nameof(Warehouses));
            }

            return View(warehouse);
        }

        // POST: Admin/RestoreWarehouse/5
        [HttpPost, ActionName("RestoreWarehouse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreWarehouseConfirmed(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdIncludingDeletedAsync(id);
            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kho.";
                return RedirectToAction(nameof(Warehouses), new { showInactive = true });
            }

            if (!warehouse.IsDeleted)
            {
                TempData["WarningMessage"] = "Kho này chưa bị xóa.";
                return RedirectToAction(nameof(Warehouses));
            }

            try
            {
                // Restore warehouse
                await _unitOfWork.Warehouses.RestoreDeletedAsync(id);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Kho '{warehouse.Name}' đã được khôi phục thành công.";
                return RedirectToAction(nameof(Warehouses));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Khôi phục kho thất bại: {ex.Message}";
                return RedirectToAction(nameof(Warehouses), new { showInactive = true });
            }
        }

        #endregion

        //#region Reports

        //// GET: Admin/Reports
        //public async Task<IActionResult> Reports()
        //{
        //    try
        //    {
        //        // Get basic statistics
        //        var assets = await _unitOfWork.Assets.GetAllAsync();
        //        var totalAssets = assets.Count();
        //        var totalAssetsValue = assets.Sum(a => a.Price);

        //        var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
        //        var activeAssets = warehouseAssets.Where(wa => (wa.GoodQuantity ?? 0) > 0).Count();
        //        var borrowedAssets = warehouseAssets.Where(wa => (wa.BorrowedGoodQuantity ?? 0) > 0).Count();
        //        var disposedAssets = warehouseAssets.Where(wa => (wa.DisposedQuantity ?? 0) > 0).Count();

        //        // Get assets by category
        //        var categories = await _unitOfWork.AssetCategories.GetAllAsync();
        //        var assetsByCategory = categories.ToDictionary(
        //            c => c.Name,
        //            c => c.Assets.Count
        //        );

        //        // Get users by role
        //        var roles = await _unitOfWork.Roles.GetAllRolesAsync();
        //        var usersByRole = new Dictionary<string, int>();
        //        foreach (var role in roles)
        //        {
        //            usersByRole[role.Name] = await _unitOfWork.Roles.CountUsersByRoleAsync(role.Id);
        //        }

        //        // Get assets by warehouse
        //        var warehouses = await _unitOfWork.Warehouses.GetWarehousesWithAssets();
        //        var assetsByWarehouse = warehouses.ToDictionary(
        //            w => w.Name,
        //            w => w.WarehouseAssets.Select(wa => wa.AssetId).Distinct().Count()
        //        );

        //        // Get borrow tickets by month (current year)
        //        var currentYear = DateTime.Now.Year;
        //        var borrowTicketsByMonth = await _unitOfWork.BorrowTickets.GetBorrowTicketStatisticsByMonth(currentYear);

        //        var viewModel = new AdminReportViewModel
        //        {
        //            TotalAssets = totalAssets,
        //            TotalAssetsValue = totalAssetsValue,
        //            ActiveAssets = activeAssets,
        //            BorrowedAssets = borrowedAssets,
        //            DisposedAssets = disposedAssets,
        //            AssetsByCategory = assetsByCategory,
        //            UsersByRole = usersByRole,
        //            AssetsByWarehouse = assetsByWarehouse,
        //            BorrowTicketsByMonth = borrowTicketsByMonth
        //        };

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu báo cáo: {ex.Message}";
        //        return View(new AdminReportViewModel());
        //    }
        //}

        //#endregion
    }
}