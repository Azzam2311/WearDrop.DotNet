using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class PromoController : Controller
{
    private readonly AppDbContext _db;

    public PromoController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Promos.Include(p => p.Transaksis).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.KodePromo.Contains(s) || (p.Deskripsi != null && p.Deskripsi.Contains(s)));
        }

        ViewBag.Search = search;
        var list = await query.OrderByDescending(p => p.IdPromo).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Promo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Promo model)
    {
        model.KodePromo = (model.KodePromo ?? string.Empty).Trim().ToUpperInvariant();

        if (await _db.Promos.AnyAsync(p => p.KodePromo == model.KodePromo))
        {
            ModelState.AddModelError(nameof(Promo.KodePromo), "Kode promo sudah terdaftar.");
        }

        if (model.TanggalSelesai.Date < model.TanggalMulai.Date)
        {
            ModelState.AddModelError(nameof(Promo.TanggalSelesai), "Tanggal selesai harus sama atau setelah tanggal mulai.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;
        _db.Promos.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Kode promo '{model.KodePromo}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id)
    {
        var item = await _db.Promos.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, Promo model)
    {
        var existing = await _db.Promos.FindAsync(id);
        if (existing == null) return NotFound();

        model.KodePromo = (model.KodePromo ?? string.Empty).Trim().ToUpperInvariant();

        if (await _db.Promos.AnyAsync(p => p.KodePromo == model.KodePromo && p.IdPromo != id))
        {
            ModelState.AddModelError(nameof(Promo.KodePromo), "Kode promo sudah digunakan promo lain.");
        }

        if (model.TanggalSelesai.Date < model.TanggalMulai.Date)
        {
            ModelState.AddModelError(nameof(Promo.TanggalSelesai), "Tanggal selesai harus sama atau setelah tanggal mulai.");
        }

        if (!ModelState.IsValid)
        {
            model.IdPromo = id;
            return View(model);
        }

        existing.KodePromo = model.KodePromo;
        existing.Deskripsi = model.Deskripsi;
        existing.Diskon = model.Diskon;
        existing.TanggalMulai = model.TanggalMulai.Date;
        existing.TanggalSelesai = model.TanggalSelesai.Date;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Kode promo '{existing.KodePromo}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var item = await _db.Promos.FindAsync(id);
        if (item == null) return NotFound();

        _db.Promos.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Promo '{item.KodePromo}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}
