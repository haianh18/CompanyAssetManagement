using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.Handover;
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
    [Authorize(Roles = "GeneralUser")]
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

            return View("~/Views/GeneralUser/Dashboard.cshtml");
        }

        // GET: GeneralUser/MyBorrowRequests
        public async Task<IActionResult> MyBorrowRequests()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Thêm logging để debug
            Console.WriteLine($"Current user: {currentUser.Id}, {currentUser.Email}, {currentUser.UserName}");

            var requests = await _unitOfWork.BorrowTickets.GetBorrowTicketsByUser(currentUser.Id);

            // Thêm logging để debug
            Console.WriteLine($"Requests count: {requests?.Count() ?? 0}");

            var viewModel = new MyBorrowRequestViewModel();

            if (requests != null && requests.Any())
            {
                viewModel.AllRequests = requests;
                viewModel.PendingRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Pending).ToList();
                viewModel.ApprovedRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Approved && !r.IsReturned).ToList();
                viewModel.RejectedRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Rejected).ToList();
                viewModel.ReturnedRequests = requests.Where(r => r.IsReturned).ToList();

                // Calculate overdue requests
                var currentDate = DateTime.Now;
                viewModel.OverdueRequests = requests
                    .Where(r => r.ApproveStatus == TicketStatus.Approved &&
                               !r.IsReturned &&
                               r.ReturnDate < currentDate)
                    .ToList();
            }

            return View(viewModel);
        }

        // GET: GeneralUser/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);

            // Check if user is eligible for borrowing (no overdue items)
            if (currentUser != null && !await _unitOfWork.BorrowTickets.IsUserEligibleForBorrowing(currentUser.Id))
            {
                TempData["ErrorMessage"] = "Bạn có tài sản đã quá hạn trả. Vui lòng trả lại tài sản trước khi mượn mới.";
                return RedirectToAction("MyBorrowRequests");
            }

            // Get all warehouse assets that are available for borrowing
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithAvailableQuantity();

            // Create SelectList for dropdown in the view
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");
            ViewBag.CurrentUserId = currentUser?.Id;

            return View();
        }

        // POST: GeneralUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowRequestViewModel model)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate request date range (7-30 days)
                    var minReturnDate = DateTime.Today.AddDays(7);
                    var maxReturnDate = DateTime.Today.AddDays(30);

                    if (model.ReturnDate < minReturnDate)
                    {
                        ModelState.AddModelError("ReturnDate", "Thời gian mượn tối thiểu là 7 ngày.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    if (model.ReturnDate > maxReturnDate)
                    {
                        ModelState.AddModelError("ReturnDate", "Thời gian mượn tối đa là 30 ngày.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    // Get the warehouse asset to check availability
                    var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(model.WarehouseAssetId);
                    if (warehouseAsset == null)
                    {
                        ModelState.AddModelError("", "Tài sản không tồn tại.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    // Check if requested quantity is available
                    int availableQuantity = (warehouseAsset.GoodQuantity ?? 0) - (warehouseAsset.BorrowedGoodQuantity ?? 0) - (warehouseAsset.HandedOverGoodQuantity ?? 0);
                    if (model.Quantity > availableQuantity)
                    {
                        ModelState.AddModelError("Quantity", $"Số lượng yêu cầu vượt quá số lượng hiện có. Hiện tại chỉ còn {availableQuantity} sản phẩm khả dụng.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    // Create borrow ticket
                    var borrowTicket = new BorrowTicket
                    {
                        WarehouseAssetId = model.WarehouseAssetId,
                        BorrowById = currentUser.Id,
                        OwnerId = null, // Will be set by warehouse manager during approval
                        Quantity = model.Quantity,
                        ReturnDate = model.ReturnDate,
                        Note = model.Note,
                        ApproveStatus = TicketStatus.Pending,
                        BorrowedAssetStatus = AssetStatus.GOOD, // Only good assets can be borrowed
                        DateCreated = DateTime.Now
                    };

                    await _unitOfWork.BorrowTickets.AddAsync(borrowTicket);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Yêu cầu mượn tài sản đã được tạo thành công và đang chờ phê duyệt.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                    await PrepareCreateViewBag(currentUser.Id);
                }
            }

            await PrepareCreateViewBag(currentUser.Id);
            return View(model);
        }

        private async Task PrepareCreateViewBag(int currentUserId)
        {
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithAvailableQuantity();
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.MinReturnDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            ViewBag.MaxReturnDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        }

        // GET: BorrowRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: GeneralUser/RequestExtension/5
        public async Task<IActionResult> RequestExtension(int id)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowTicket = await _unitOfWork.BorrowTickets.GetBorrowTicketWithExtensions(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Check if user is the borrower
            if (borrowTicket.BorrowById != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền yêu cầu gia hạn cho phiếu mượn này.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if ticket can be extended (not already extended)
            if (borrowTicket.IsExtended)
            {
                TempData["ErrorMessage"] = "Phiếu mượn này đã được gia hạn trước đó. Mỗi phiếu mượn chỉ được gia hạn một lần.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if ticket is approved
            if (borrowTicket.ApproveStatus != TicketStatus.Approved)
            {
                TempData["ErrorMessage"] = "Chỉ có thể gia hạn phiếu mượn đã được phê duyệt.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if ticket has pending extension request
            if (borrowTicket.ExtensionApproveStatus == TicketStatus.Pending)
            {
                TempData["InfoMessage"] = "Yêu cầu gia hạn của bạn đang chờ xét duyệt.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if return date is at least 7 days from now
            if (borrowTicket.ReturnDate < DateTime.Now.AddDays(7))
            {
                TempData["ErrorMessage"] = "Chỉ có thể yêu cầu gia hạn trước thời hạn trả ít nhất 7 ngày.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            var model = new ExtensionRequestViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                OriginalBorrowDate = borrowTicket.DateCreated,
                CurrentReturnDate = borrowTicket.ReturnDate,
                Quantity = borrowTicket.Quantity,
                // Default extension to 30 days from current return date
                NewReturnDate = borrowTicket.ReturnDate?.AddDays(30)
            };

            return View(model);
        }

        // POST: GeneralUser/RequestExtension
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExtension(ExtensionRequestViewModel model)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(model.BorrowTicketId);
                    if (borrowTicket == null)
                    {
                        return NotFound();
                    }

                    // Check all the validation rules again in case of form manipulation
                    if (borrowTicket.BorrowById != currentUser.Id ||
                        borrowTicket.IsExtended ||
                        borrowTicket.ApproveStatus != TicketStatus.Approved ||
                        borrowTicket.ExtensionApproveStatus == TicketStatus.Pending ||
                        borrowTicket.ReturnDate < DateTime.Now.AddDays(7))
                    {
                        TempData["ErrorMessage"] = "Không thể gia hạn phiếu mượn này.";
                        return RedirectToAction(nameof(MyBorrowRequests));
                    }

                    // Validate extension date
                    if (model.NewReturnDate <= borrowTicket.ReturnDate)
                    {
                        ModelState.AddModelError("NewReturnDate", "Ngày trả mới phải sau ngày trả hiện tại.");
                        return View(model);
                    }

                    // Validate max extension (30 days from current return date)
                    var maxExtensionDate = borrowTicket.ReturnDate.Value.AddDays(30);
                    if (model.NewReturnDate > maxExtensionDate)
                    {
                        ModelState.AddModelError("NewReturnDate", "Thời gian gia hạn tối đa là 30 ngày từ ngày trả hiện tại.");
                        return View(model);
                    }

                    // Request extension
                    await _unitOfWork.BorrowTickets.RequestExtensionAsync(model.BorrowTicketId, model.NewReturnDate.Value);
                    await _unitOfWork.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Yêu cầu gia hạn đã được gửi và đang chờ xét duyệt.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: GeneralUser/ReturnAsset/5
        public async Task<IActionResult> ReturnAsset(int id)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Check if user is the borrower
            if (borrowTicket.BorrowById != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền trả tài sản này.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if already returned
            if (borrowTicket.IsReturned)
            {
                TempData["ErrorMessage"] = "Tài sản này đã được trả trước đó.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if there's a pending return request
            var pendingReturnRequest = borrowTicket.ReturnTickets?.FirstOrDefault(r => r.ApproveStatus == TicketStatus.Pending);
            if (pendingReturnRequest != null)
            {
                TempData["ErrorMessage"] = "Tài sản này đang có yêu cầu trả đang chờ xét duyệt.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if approved
            if (borrowTicket.ApproveStatus != TicketStatus.Approved)
            {
                TempData["ErrorMessage"] = "Chỉ có thể trả tài sản đã được phê duyệt mượn.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            var model = new ReturnAssetViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                BorrowDate = borrowTicket.DateCreated,
                ScheduledReturnDate = borrowTicket.ReturnDate,
                Quantity = borrowTicket.Quantity,
                IsEarlyReturn = DateTime.Now < borrowTicket.ReturnDate
            };

            return View(model);
        }

        // GET: GeneralUser/MyReturnRequests
        public async Task<IActionResult> MyReturnRequests()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all return tickets for the current user
            var returnTickets = await _unitOfWork.ReturnTickets.GetReturnTicketsByUser(currentUser.Id);

            var viewModel = new MyReturnRequestsViewModel();

            if (returnTickets != null && returnTickets.Any())
            {
                viewModel.AllReturnRequests = returnTickets;
                viewModel.PendingReturnRequests = returnTickets.Where(r => r.ApproveStatus == TicketStatus.Pending).ToList();
                viewModel.ApprovedReturnRequests = returnTickets.Where(r => r.ApproveStatus == TicketStatus.Approved).ToList();
                viewModel.RejectedReturnRequests = returnTickets.Where(r => r.ApproveStatus == TicketStatus.Rejected).ToList();
            }

            return View(viewModel);
        }

        // GET: GeneralUser/ReturnTicketDetails/5
        public async Task<IActionResult> ReturnTicketDetails(int id)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            // Check if user is the one who created the return request
            if (returnTicket.ReturnById != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin phiếu trả này.";
                return RedirectToAction(nameof(MyReturnRequests));
            }

            return View(returnTicket);
        }

        // POST: GeneralUser/ReturnAsset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnAsset(ReturnAssetViewModel model)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Process early return
                    var returnTicket = await ProcessEarlyReturnAsync(
                        model.BorrowTicketId,
                        currentUser.Id,
                        model.Notes
                    );

                    TempData["SuccessMessage"] = "Yêu cầu trả tài sản đã được gửi. Vui lòng mang tài sản đến kho để hoàn tất quy trình.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }


        public async Task<ReturnTicket> ProcessEarlyReturnAsync(int borrowTicketId, int returnById, string notes)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null)
                throw new Exception("Borrow ticket not found");

            if (borrowTicket.IsReturned)
                throw new Exception("This borrow ticket has already been returned");

            // Create return ticket for early return (initiated by borrower)
            var returnTicket = new ReturnTicket
            {
                BorrowTicketId = borrowTicketId,
                ReturnById = returnById,
                OwnerId = borrowTicket.OwnerId,
                Quantity = borrowTicket.Quantity,
                Note = notes,
                ApproveStatus = TicketStatus.Pending,
                ReturnRequestDate = DateTime.Now,
                IsEarlyReturn = DateTime.Now < borrowTicket.ReturnDate,
                DateCreated = DateTime.Now
            };

            await _unitOfWork.ReturnTickets.AddAsync(returnTicket);
            await _unitOfWork.SaveChangesAsync();
            return returnTicket;
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

            var viewModel = new HandoverTicketsViewModel
            {
                AllHandovers = handoverTickets,
                ActiveHandovers = handoverTickets.Where(h => h.IsActive).ToList(),
                InactiveHandovers = handoverTickets.Where(h => !h.IsActive).ToList()
            };

            return View(viewModel);
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