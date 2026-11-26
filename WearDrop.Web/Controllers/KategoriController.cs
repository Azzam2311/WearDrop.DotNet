using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class KategoriController : Controller
{
    private readonly AppDbContext _db;

    public KategoriController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Kategoris.Include(k => k.Produks).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(k => k.NamaKategori.Contains(s));
        }

        ViewBag.Search = search;
        var list = await query.OrderBy(k => k.NamaKategori).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Kategori());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kategori model)
    {
        model.NamaKategori = (model.NamaKategori ?? string.Empty).Trim();

        if (await _db.Kategoris.AnyAsync(k => k.NamaKategori == model.NamaKategori))
        {
            ModelState.AddModelError(nameof(Kategori.NamaKategori), "Nama kategori sudah terdaftar.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;
        _db.Kategoris.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Kategori '{model.NamaKategori}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id)
    {
        var item = await _db.Kategoris.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, Kategori model)
    {
        var existing = await _db.Kategoris.FindAsync(id);
        if (existing == null) return NotFound();

        model.NamaKategori = (model.NamaKategori ?? string.Empty).Trim();

        if (await _db.Kategoris.AnyAsync(k => k.NamaKategori == model.NamaKategori && k.IdKategori != id))
        {
            ModelState.AddModelError(nameof(Kategori.NamaKategori), "Nama kategori sudah digunakan.");
        }

        if (!ModelState.IsValid)
        {
            model.IdKategori = id;
            return View(model);
        }

        existing.NamaKategori = model.NamaKategori;
        existing.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Kategori '{existing.NamaKategori}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var item = await _db.Kategoris.Include(k => k.Produks).FirstOrDefaultAsync(k => k.IdKategori == id);
        if (item == null) return NotFound();

        if (item.Produks.Count > 0)
        {
            TempData["Error"] = $"Kategori '{item.NamaKategori}' tidak dapat dihapus karena masih memiliki {item.Produks.Count} produk.";
            return RedirectToAction(nameof(Index));
        }

        _db.Kategoris.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Kategori '{item.NamaKategori}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}
