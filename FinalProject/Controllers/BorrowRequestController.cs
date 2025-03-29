// 1. Update BorrowRequestController
using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.ReturnRequest;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class BorrowRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BorrowRequestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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



    }
}