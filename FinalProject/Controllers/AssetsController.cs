using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class AssetsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssetsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Assets
        public async Task<IActionResult> Index()
        {
            var assets = await _unitOfWork.Assets.GetAllIncludingDeletedAsync();
            return View(assets);
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _unitOfWork.Assets.GetByIdIncludingDeletedAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            ViewData["AssetCategoryId"] = new SelectList(_unitOfWork.AssetCategories.GetAllAsync().Result, "Id", "Name");
            return View();
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Unit,ActiveStatus,AssetCategoryId,AssetStatus,DateCreated,DateModified,Description,Note")] Asset asset)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Assets.AddAsync(asset);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetCategoryId"] = new SelectList(_unitOfWork.AssetCategories.GetAllAsync().Result, "Id", "Name", asset.AssetCategoryId);
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _unitOfWork.Assets.GetByIdAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }
            ViewData["AssetCategoryId"] = new SelectList(_unitOfWork.AssetCategories.GetAllAsync().Result, "Id", "Name", asset.AssetCategoryId);
            return View(asset);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Unit,ActiveStatus,AssetCategoryId,AssetStatus,DateCreated,DateModified,Description,Note")] Asset asset)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Assets.Update(asset);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await AssetExists(asset.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetCategoryId"] = new SelectList(_unitOfWork.AssetCategories.GetAllAsync().Result, "Id", "Name", asset.AssetCategoryId);
            return View(asset);
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _unitOfWork.Assets.GetByIdAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.Assets.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _unitOfWork.Assets.GetByIdIncludingDeletedAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            await _unitOfWork.Assets.RestoreDeletedAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> AssetExists(int id)
        {
            return await _unitOfWork.Assets.ExistsAsync(a => a.Id == id);
        }
    }
}


