using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.ReturnRequest;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "GENERAL_USER")]
    public class GeneralUserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public GeneralUserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: GeneralUser/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get borrow requests
            var borrowRequests = await _unitOfWork.BorrowTickets.GetBorrowTicketsByUser(currentUser.Id);

            // Count by status
            var pendingRequests = borrowRequests.Count(b => b.ApproveStatus == TicketStatus.Pending);
            var approvedRequests = borrowRequests.Count(b => b.ApproveStatus == TicketStatus.Approved && !b.IsReturned);
            var rejectedRequests = borrowRequests.Count(b => b.ApproveStatus == TicketStatus.Rejected);
            var returnedRequests = borrowRequests.Count(b => b.IsReturned);

            // Count overdue requests
            var overdueRequests = borrowRequests.Count(b =>
                b.ApproveStatus == TicketStatus.Approved &&
                !b.IsReturned &&
                b.ReturnDate < DateTime.Now);

            // Get handover tickets
            var handoverTickets = await _unitOfWork.HandoverTickets.GetHandoverTicketsByHandoverTo(currentUser.Id);
            var activeHandovers = handoverTickets.Count(h => h.IsActive);

            // Recent borrow requests
            var recentBorrows = borrowRequests
                .OrderByDescending(b => b.DateCreated)
                .Take(5)
                .ToList();

            // Set ViewBag values for dashboard
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.ApprovedRequests = approvedRequests;
            ViewBag.RejectedRequests = rejectedRequests;
            ViewBag.ReturnedRequests = returnedRequests;
            ViewBag.OverdueRequests = overdueRequests;
            ViewBag.ActiveHandovers = activeHandovers;
            ViewBag.RecentBorrows = recentBorrows;

            return View();
        }

        // GET: GeneralUser/MyBorrowRequests
        public async Task<IActionResult> MyBorrowRequests()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _unitOfWork.BorrowTickets.GetBorrowTicketsByUser(currentUser.Id);

            // Group by status
            ViewBag.PendingRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Pending).ToList();
            ViewBag.ApprovedRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Approved && !r.IsReturned).ToList();
            ViewBag.RejectedRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Rejected).ToList();
            ViewBag.ReturnedRequests = requests.Where(r => r.IsReturned).ToList();

            // Calculate overdue requests
            var currentDate = DateTime.Now;
            ViewBag.OverdueRequests = requests
                .Where(r => r.ApproveStatus == TicketStatus.Approved &&
                           !r.IsReturned &&
                           r.ReturnDate < currentDate)
                .ToList();

            return View(requests);
        }

        // GET: GeneralUser/Create (Redirects to BorrowRequest/Create)
        public IActionResult Create()
        {
            return RedirectToAction("Create", "BorrowRequest");
        }

        // GET: GeneralUser/RequestExtension (Redirects to BorrowRequest/RequestExtension)
        public IActionResult RequestExtension(int id)
        {
            return RedirectToAction("RequestExtension", "BorrowRequest", new { id = id });
        }

        // GET: GeneralUser/ReturnAsset (Redirects to BorrowRequest/ReturnAsset)
        public IActionResult ReturnAsset(int id)
        {
            return RedirectToAction("ReturnAsset", "BorrowRequest", new { id = id });
        }

        // GET: GeneralUser/Details (Redirects to BorrowRequest/Details)
        public IActionResult Details(int id)
        {
            return RedirectToAction("Details", "BorrowRequest", new { id = id });
        }

        // GET: GeneralUser/MyAssignedAssets
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

            return View(handoverTickets);
        }

        // GET: GeneralUser/HandoverTicketDetails
        public async Task<IActionResult> HandoverTicketDetails(int id)
        {
            var handoverTicket = await _unitOfWork.HandoverTickets.GetHandoverTicketWithDetails(id);
            if (handoverTicket == null)
            {
                return NotFound();
            }

            // Verify the current user is the one this ticket is assigned to
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null || handoverTicket.HandoverToId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin này.";
                return RedirectToAction(nameof(MyAssignedAssets));
            }

            return View(handoverTicket);
        }

        // GET: GeneralUser/ChangePassword (Redirects to Account/ChangePassword)
        public IActionResult ChangePassword()
        {
            return RedirectToAction("ChangePassword", "Account");
        }
    }
}