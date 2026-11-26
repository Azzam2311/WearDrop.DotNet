using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class PenggunaController : Controller
{
    private static readonly string[] ValidRoles = ["admin", "kasir", "karyawan"];
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AdminUser> _hasher = new();

    public PenggunaController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, string? role)
    {
        var query = _db.Admins.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(a => a.Nama.Contains(s) || a.Email.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role) && ValidRoles.Contains(role))
        {
            query = query.Where(a => a.Role == role);
        }

        ViewBag.Search = search;
        ViewBag.SelectedRole = role;
        ViewBag.TotalAdmin = await _db.Admins.CountAsync(a => a.Role == "admin");
        ViewBag.TotalKasir = await _db.Admins.CountAsync(a => a.Role == "kasir");
        ViewBag.TotalKaryawan = await _db.Admins.CountAsync(a => a.Role == "karyawan");

        var list = await query.ToListAsync();
        list = list
            .OrderBy(a => a.Role switch { "admin" => 1, "kasir" => 2, _ => 3 })
            .ThenBy(a => a.Nama)
            .ToList();

        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AdminUser());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUser model, string? plainPassword)
    {
        model.Nama = (model.Nama ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (!ValidRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(AdminUser.Role), "Pilih role yang valid (admin, kasir, atau karyawan).");
        }

        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 8)
        {
            ModelState.AddModelError("plainPassword", "Password wajib diisi minimal 8 karakter.");
        }

        if (await _db.Admins.AnyAsync(a => a.Email == model.Email))
        {
            ModelState.AddModelError(nameof(AdminUser.Email), "Email sudah terdaftar pada staf lain.");
        }

        ModelState.Remove(nameof(AdminUser.Password));
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Password = _hasher.HashPassword(model, plainPassword!);
        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;

        _db.Admins.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Akun staf '{model.Nama}' ({model.Role.ToUpperInvariant()}) berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id)
    {
        var item = await _db.Admins.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, AdminUser model, string? newPassword)
    {
        var existing = await _db.Admins.FindAsync(id);
        if (existing == null) return NotFound();

        model.Nama = (model.Nama ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (!ValidRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(AdminUser.Role), "Pilih role yang valid.");
        }

        if (await _db.Admins.AnyAsync(a => a.Email == model.Email && a.IdAdmin != id))
        {
            ModelState.AddModelError(nameof(AdminUser.Email), "Email sudah digunakan staf lain.");
        }

        if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length < 8)
        {
            ModelState.AddModelError("newPassword", "Password baru minimal 8 karakter.");
        }

        if (existing.Role == "admin" && model.Role != "admin")
        {
            var adminCount = await _db.Admins.CountAsync(a => a.Role == "admin");
            if (adminCount <= 1)
            {
                ModelState.AddModelError(nameof(AdminUser.Role), "Role tidak dapat diubah karena ini adalah satu-satunya akun Admin.");
            }
        }

        ModelState.Remove(nameof(AdminUser.Password));
        if (!ModelState.IsValid)
        {
            model.IdAdmin = id;
            return View(model);
        }

        existing.Nama = model.Nama;
        existing.Email = model.Email;
        existing.Role = model.Role;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            existing.Password = _hasher.HashPassword(existing, newPassword);
        }
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Data staf '{existing.Nama}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var item = await _db.Admins.FindAsync(id);
        if (item == null) return NotFound();

        if (item.Role == "admin" && await _db.Admins.CountAsync(a => a.Role == "admin") <= 1)
        {
            TempData["Error"] = "Akun Admin terakhir tidak dapat dihapus.";
            return RedirectToAction(nameof(Index));
        }

        _db.Admins.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Akun staf '{item.Nama}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}
