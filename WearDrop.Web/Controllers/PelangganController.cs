using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class PelangganController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Pelanggan> _hasher = new();

    public PelangganController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Pelanggans.Include(p => p.Transaksis).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Nama.Contains(s) || p.Email.Contains(s) || p.NoHp.Contains(s));
        }

        ViewBag.Search = search;
        var list = await query.OrderByDescending(p => p.IdPelanggan).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Pelanggan());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pelanggan model, string? plainPassword)
    {
        model.Nama = (model.Nama ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        model.NoHp = (model.NoHp ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
        {
            ModelState.AddModelError("plainPassword", "Password wajib diisi minimal 6 karakter.");
        }

        if (await _db.Pelanggans.AnyAsync(p => p.Email == model.Email))
        {
            ModelState.AddModelError(nameof(Pelanggan.Email), "Email pelanggan sudah terdaftar.");
        }

        ModelState.Remove(nameof(Pelanggan.Password));
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Password = _hasher.HashPassword(model, plainPassword!);
        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;

        _db.Pelanggans.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Pelanggan '{model.Nama}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id)
    {
        var item = await _db.Pelanggans.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, Pelanggan model, string? newPassword)
    {
        var existing = await _db.Pelanggans.FindAsync(id);
        if (existing == null) return NotFound();

        model.Nama = (model.Nama ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
        model.NoHp = (model.NoHp ?? string.Empty).Trim();

        if (await _db.Pelanggans.AnyAsync(p => p.Email == model.Email && p.IdPelanggan != id))
        {
            ModelState.AddModelError(nameof(Pelanggan.Email), "Email sudah digunakan pelanggan lain.");
        }

        ModelState.Remove(nameof(Pelanggan.Password));
        if (!ModelState.IsValid)
        {
            model.IdPelanggan = id;
            return View(model);
        }

        existing.Nama = model.Nama;
        existing.Email = model.Email;
        existing.NoHp = model.NoHp;
        existing.Alamat = model.Alamat;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            existing.Password = _hasher.HashPassword(existing, newPassword);
        }
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Data pelanggan '{existing.Nama}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var item = await _db.Pelanggans.Include(p => p.Transaksis).FirstOrDefaultAsync(p => p.IdPelanggan == id);
        if (item == null) return NotFound();

        if (item.Transaksis.Count > 0)
        {
            TempData["Error"] = $"Pelanggan '{item.Nama}' tidak dapat dihapus karena memiliki {item.Transaksis.Count} riwayat transaksi.";
            return RedirectToAction(nameof(Index));
        }

        _db.Pelanggans.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Pelanggan '{item.Nama}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}
