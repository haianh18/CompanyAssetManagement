using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class DirectLoginController : Controller
    {
        private readonly CompanyAssetManagementContext _context;

        public DirectLoginController(CompanyAssetManagementContext context)
        {
            _context = context;
        }

        // GET: DirectLogin
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string roleType)
        {
            // Lấy người dùng trực tiếp từ database mà không qua Identity
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra vai trò
            if (user.Role.RoleType.ToString() == roleType)
            {
                switch (roleType)
                {
                    case "ADMIN":
                        return RedirectToAction("Dashboard", "Admin");
                    case "WAREHOUSE_MANAGER":
                        return RedirectToAction("Dashboard", "WarehouseManager");
                    default:
                        return RedirectToAction("Dashboard", "GeneralUser");
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}