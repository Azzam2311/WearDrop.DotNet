using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class KeranjangController : Controller
{
    private readonly AppDbContext _db;

    public KeranjangController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Keranjangs
            .Include(k => k.Pelanggan)
            .Include(k => k.Produk)
                .ThenInclude(p => p!.Kategori)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(k =>
                (k.Pelanggan != null && k.Pelanggan.Nama.Contains(s)) ||
                (k.Produk != null && k.Produk.NamaProduk.Contains(s)));
        }

        ViewBag.Search = search;
        ViewBag.Pelanggans = await _db.Pelanggans.OrderBy(p => p.Nama).ToListAsync();
        ViewBag.Produks = await _db.Produks.Where(p => p.Stok > 0).OrderBy(p => p.NamaProduk).ToListAsync();

        var list = await query.OrderByDescending(k => k.IdKeranjang).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ulong idPelanggan, ulong idProduk, int jumlah)
    {
        if (jumlah < 1) jumlah = 1;

        var existing = await _db.Keranjangs
            .FirstOrDefaultAsync(k => k.IdPelanggan == idPelanggan && k.IdProduk == idProduk);

        if (existing != null)
        {
            existing.Jumlah += jumlah;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            _db.Keranjangs.Add(new Keranjang
            {
                IdPelanggan = idPelanggan,
                IdProduk = idProduk,
                Jumlah = jumlah,
                TanggalTambah = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Item keranjang berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var item = await _db.Keranjangs.FindAsync(id);
        if (item != null)
        {
            _db.Keranjangs.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Item keranjang berhasil dihapus.";
        }
        return RedirectToAction(nameof(Index));
    }
}
