using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class HandoverTicketController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HandoverTicketController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: HandoverTicket/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var handoverTicket = await _unitOfWork.HandoverTickets.GetHandoverTicketWithDetails(id);
            if (handoverTicket == null)
            {
                return NotFound();
            }

            return View(handoverTicket);
        }

        // GET: HandoverTicket/MyAssignedAssets
        public async Task<IActionResult> MyAssignedAssets()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var handoverTickets = await _unitOfWork.HandoverTickets.GetHandoverTicketsByHandoverTo(currentUser.Id);

            // Group by active/inactive
            ViewBag.ActiveHandovers = handoverTickets.Where(h => h.IsActive).ToList();
            ViewBag.InactiveHandovers = handoverTickets.Where(h => !h.IsActive).ToList();

            return View("~/Views/GeneralUser/MyAssignedAssets.cshtml");
        }
    }
}