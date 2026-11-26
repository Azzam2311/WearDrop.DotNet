using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class ProdukController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProdukController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index(string? search, ulong? kategori, string? filterStok)
    {
        var query = _db.Produks.Include(p => p.Kategori).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.NamaProduk.Contains(s) || (p.Deskripsi != null && p.Deskripsi.Contains(s)));
        }

        if (kategori.HasValue && kategori.Value > 0)
        {
            query = query.Where(p => p.IdKategori == kategori.Value);
        }

        if (filterStok == "habis")
        {
            query = query.Where(p => p.Stok <= 0);
        }
        else if (filterStok == "kritis")
        {
            query = query.Where(p => p.Stok <= 5);
        }

        ViewBag.Search = search;
        ViewBag.SelectedKategori = kategori;
        ViewBag.FilterStok = filterStok;
        ViewBag.Kategoris = await _db.Kategoris.OrderBy(k => k.NamaKategori).ToListAsync();

        var list = await query.OrderByDescending(p => p.IdProduk).ToListAsync();
        return View(list);
    }

    private async Task LoadKategoriDropdown(ulong? selected = null)
    {
        var kategoris = await _db.Kategoris.OrderBy(k => k.NamaKategori).ToListAsync();
        ViewBag.KategoriList = new SelectList(kategoris, "IdKategori", "NamaKategori", selected);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadKategoriDropdown();
        return View(new Produk());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Produk model, IFormFile? fileGambar)
    {
        if (!ModelState.IsValid)
        {
            await LoadKategoriDropdown(model.IdKategori);
            return View(model);
        }

        if (fileGambar != null && fileGambar.Length > 0)
        {
            model.Gambar = await SaveUploadedImage(fileGambar);
        }

        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;
        _db.Produks.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Produk '{model.NamaProduk}' berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(ulong id)
    {
        var item = await _db.Produks.FindAsync(id);
        if (item == null) return NotFound();

        await LoadKategoriDropdown(item.IdKategori);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ulong id, Produk model, IFormFile? fileGambar)
    {
        var existing = await _db.Produks.FindAsync(id);
        if (existing == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.IdProduk = id;
            await LoadKategoriDropdown(model.IdKategori);
            return View(model);
        }

        existing.IdKategori = model.IdKategori;
        existing.NamaProduk = model.NamaProduk.Trim();
        existing.Harga = model.Harga;
        existing.Stok = model.Stok;
        existing.Deskripsi = model.Deskripsi;

        if (fileGambar != null && fileGambar.Length > 0)
        {
            existing.Gambar = await SaveUploadedImage(fileGambar);
        }
        else if (!string.IsNullOrWhiteSpace(model.Gambar))
        {
            existing.Gambar = model.Gambar.Trim();
        }

        existing.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Produk '{existing.NamaProduk}' berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStok(ulong id, int stok)
    {
        var existing = await _db.Produks.FindAsync(id);
        if (existing == null) return NotFound();

        if (stok < 0)
        {
            TempData["Error"] = "Stok tidak boleh kurang dari 0.";
            return RedirectToAction(nameof(Index));
        }

        existing.Stok = stok;
        existing.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Stok '{existing.NamaProduk}' diperbarui menjadi {stok}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var item = await _db.Produks.Include(p => p.DetailTransaksis).FirstOrDefaultAsync(p => p.IdProduk == id);
        if (item == null) return NotFound();

        if (item.DetailTransaksis.Count > 0)
        {
            TempData["Error"] = $"Produk '{item.NamaProduk}' tidak dapat dihapus karena tercatat pada {item.DetailTransaksis.Count} transaksi. Ubah stok ke 0 jika habis.";
            return RedirectToAction(nameof(Index));
        }

        _db.Produks.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Produk '{item.NamaProduk}' berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveUploadedImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "produk");
        Directory.CreateDirectory(uploadDir);

        var fullPath = Path.Combine(uploadDir, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/produk/{fileName}";
    }
}
