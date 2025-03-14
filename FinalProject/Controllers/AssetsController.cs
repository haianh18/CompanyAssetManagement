using FinalProject.Models;
using FinalProject.Services;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        // GET: Assets
        public async Task<IActionResult> Index()
        {
            var assets = await _assetService.GetAllAsync();
            return View(assets);
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // GET: Assets/Create
        public async Task<IActionResult> Create()
        {
            ViewData["AssetCategoryId"] = new SelectList(await _assetService.GetAllAsync(), "Id", "Name");
            return View();
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Unit,ActiveStatus,AssetCategoryId,AssetStatus,DateCreated,DateModified,Description,Note")] Asset asset)
        {
            if (ModelState.IsValid)
            {
                await _assetService.AddAsync(asset);
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetCategoryId"] = new SelectList(await _assetService.GetAllAsync(), "Id", "Name", asset.AssetCategoryId);
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }
            ViewData["AssetCategoryId"] = new SelectList(await _assetService.GetAllAsync(), "Id", "Name", asset.AssetCategoryId);
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
                    await _assetService.UpdateAsync(asset);
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
            ViewData["AssetCategoryId"] = new SelectList(await _assetService.GetAllAsync(), "Id", "Name", asset.AssetCategoryId);
            return View(asset);
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdAsync(id.Value);
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
            await _assetService.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdAsync(id.Value);
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
            await _assetService.RestoreAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> AssetExists(int id)
        {
            return await _assetService.GetByIdAsync(id) != null;
        }
    }
}

