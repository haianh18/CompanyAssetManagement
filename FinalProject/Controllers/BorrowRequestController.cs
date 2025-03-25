using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    public class BorrowRequestController : Controller
    {
        private readonly IBorrowTicketService _borrowTicketService;
        public async Task<IActionResult> BorrowRequests()
        {
            var requests = await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync();
            return View(requests);
        }

        public async Task<IActionResult> ApproveBorrowRequest(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            // Logic to approve borrow request
            // This might involve changing status, updating warehouse assets, etc.
            return RedirectToAction(nameof(BorrowRequests));
        }

        public async Task<IActionResult> RejectBorrowRequest(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            // Logic to reject borrow request
            // This might involve changing status, etc.
            return RedirectToAction(nameof(BorrowRequests));
        }
    }
}
