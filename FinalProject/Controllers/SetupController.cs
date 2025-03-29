using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SetupController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly CompanyAssetManagementContext _context;

    public SetupController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, CompanyAssetManagementContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        try
        {
            // Xóa user cũ nếu tồn tại
            var existingUser = await _userManager.FindByEmailAsync("test@example.com");
            if (existingUser != null)
            {
                await _userManager.DeleteAsync(existingUser);
            }

            // Tạo user mới
            var newUser = new AppUser
            {
                UserName = "test",
                Email = "test@example.com",
                FullName = "Test User",
                RoleId = 2,  // Warehouse Manager
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, "Test@123");
            string resultInfo = result.Succeeded
                ? "User created successfully"
                : "Failed: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return Content($"Create user result: {resultInfo}");
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userInfo = users.Select(u => new { u.Id, u.UserName, u.Email, u.RoleId, u.EmailConfirmed }).ToList();
        return Json(userInfo);
    }

    [HttpGet]
    public async Task<IActionResult> TestUserManager()
    {
        try
        {
            // Tạo một instance user tạm thời
            var tempUser = new AppUser
            {
                UserName = "tempuser",
                Email = "temp@example.com",
                EmailConfirmed = true,
                RoleId = 2,  // Warehouse Manager
                FullName = "Temp User"  // Thêm giá trị cho FullName
            };

            // Lưu tạm thời
            await _userManager.CreateAsync(tempUser, "Temp@123");

            // Kiểm tra xem có tìm được user không
            var foundByEmail = await _userManager.FindByEmailAsync("temp@example.com");
            var foundByName = await _userManager.FindByNameAsync("tempuser");

            // Xóa user tạm
            if (foundByEmail != null)
                await _userManager.DeleteAsync(foundByEmail);

            return Content($"UserManager test:\n" +
                          $"Found by email: {(foundByEmail != null ? "Yes" : "No")}\n" +
                          $"Found by name: {(foundByName != null ? "Yes" : "No")}");
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateSimpleUser()
    {
        try
        {
            var user = new AppUser
            {
                UserName = "simpleuser",
                Email = "simple@example.com",
                EmailConfirmed = true,
                RoleId = 2,  // Warehouse Manager
                FullName = "Simple User"  // Thêm giá trị cho FullName
            };

            var result = await _userManager.CreateAsync(user, "Password123!");

            return Content($"Create user result: {(result.Succeeded ? "Success" : "Failed")}\n" +
                          $"Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}");
        }
    }

    [HttpGet]
    public IActionResult CheckDatabase()
    {
        try
        {
            // Kiểm tra kết nối và đọc dữ liệu
            var users = _context.Users.ToList();
            var userEmails = users.Select(u => u.Email).ToList();

            // Kiểm tra xem email có tồn tại
            bool hasManagerEmail = userEmails.Contains("manager@example.com");

            return Content($"Database connection: Success\n" +
                           $"Total users: {users.Count}\n" +
                           $"User emails: {string.Join(", ", userEmails)}\n" +
                           $"Has manager@example.com: {hasManagerEmail}");
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}");
        }
    }
}