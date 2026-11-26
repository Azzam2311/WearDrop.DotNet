using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class TransaksiController : Controller
{
    private readonly AppDbContext _db;

    public TransaksiController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, string? statusTransaksi, string? statusPembayaran)
    {
        var query = _db.Transaksis
            .Include(t => t.Pelanggan)
            .Include(t => t.Admin)
            .Include(t => t.Pembayaran)
            .Include(t => t.Pengiriman)
            .Include(t => t.DetailTransaksis)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            ulong parsedId = 0;
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(digits)) ulong.TryParse(digits[^Math.Min(4, digits.Length)..], out parsedId);

            query = query.Where(t =>
                (parsedId > 0 && t.IdTransaksi == parsedId) ||
                (t.Pelanggan != null && (t.Pelanggan.Nama.Contains(s) || t.Pelanggan.NoHp.Contains(s))));
        }

        if (!string.IsNullOrWhiteSpace(statusTransaksi))
        {
            query = query.Where(t => t.StatusTransaksi == statusTransaksi);
        }

        if (!string.IsNullOrWhiteSpace(statusPembayaran))
        {
            query = query.Where(t => t.Pembayaran != null && t.Pembayaran.StatusPembayaran == statusPembayaran);
        }

        ViewBag.Search = search;
        ViewBag.StatusTransaksi = statusTransaksi;
        ViewBag.StatusPembayaran = statusPembayaran;

        var list = await query.OrderByDescending(t => t.TanggalTransaksi).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadPosLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ulong idPelanggan,
        ulong idProduk,
        int jumlah,
        ulong? idPromo,
        string metodePembayaran,
        string metodePengiriman)
    {
        var pelanggan = await _db.Pelanggans.FindAsync(idPelanggan);
        var produk = await _db.Produks.FindAsync(idProduk);

        if (pelanggan == null || produk == null || jumlah < 1)
        {
            TempData["Error"] = "Pilih pelanggan, produk, dan jumlah minimal 1.";
            await LoadPosLists();
            return View();
        }

        if (produk.Stok < jumlah)
        {
            TempData["Error"] = $"Stok '{produk.NamaProduk}' hanya tersisa {produk.Stok} pcs.";
            await LoadPosLists();
            return View();
        }

        decimal subtotal = produk.Harga * jumlah;
        decimal potongan = 0m;

        if (idPromo.HasValue && idPromo.Value > 0)
        {
            var promo = await _db.Promos.FindAsync(idPromo.Value);
            if (promo != null && promo.IsAktif)
            {
                potongan = Math.Min(subtotal, promo.Diskon);
            }
            else
            {
                idPromo = null;
            }
        }

        var now = DateTime.Now;
        var transaksi = new Transaksi
        {
            IdPelanggan = idPelanggan,
            IdPromo = idPromo,
            TanggalTransaksi = now,
            TotalPembayaran = Math.Max(0m, subtotal - potongan),
            StatusTransaksi = "pending",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Transaksis.Add(transaksi);
        await _db.SaveChangesAsync();

        _db.DetailTransaksis.Add(new DetailTransaksi
        {
            IdTransaksi = transaksi.IdTransaksi,
            IdProduk = produk.IdProduk,
            Jumlah = jumlah,
            Harga = produk.Harga,
            Subtotal = subtotal,
            CreatedAt = now,
            UpdatedAt = now
        });

        _db.Pembayarans.Add(new Pembayaran
        {
            IdTransaksi = transaksi.IdTransaksi,
            MetodePembayaran = string.IsNullOrWhiteSpace(metodePembayaran) ? "Transfer Bank BCA" : metodePembayaran.Trim(),
            StatusPembayaran = "menunggu_verifikasi",
            CreatedAt = now,
            UpdatedAt = now
        });

        _db.Pengirimans.Add(new Pengiriman
        {
            IdTransaksi = transaksi.IdTransaksi,
            MetodePengiriman = string.IsNullOrWhiteSpace(metodePengiriman) ? "JNE Reguler" : metodePengiriman.Trim(),
            StatusPengiriman = "menunggu_konfirmasi",
            CreatedAt = now,
            UpdatedAt = now
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Transaksi {transaksi.KodeTransaksi} berhasil dibuat.";
        return RedirectToAction(nameof(Details), new { id = transaksi.IdTransaksi });
    }

    public async Task<IActionResult> Details(ulong id)
    {
        var item = await GetFullTransaksi(id);
        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Struk(ulong id)
    {
        var item = await GetFullTransaksi(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifikasiPembayaran(ulong id, string aksi)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var transaksi = await _db.Transaksis
                .Include(t => t.Pembayaran)
                .Include(t => t.DetailTransaksis)
                    .ThenInclude(d => d.Produk)
                .FirstOrDefaultAsync(t => t.IdTransaksi == id);

            if (transaksi == null) return NotFound();
            if (transaksi.Pembayaran == null)
            {
                TempData["Error"] = "Transaksi ini belum memiliki data pembayaran.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (aksi == "ditolak")
            {
                if (transaksi.Pembayaran.StatusPembayaran == "lunas")
                {
                    TempData["Error"] = "Pembayaran yang sudah LUNAS tidak dapat ditolak.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                transaksi.Pembayaran.StatusPembayaran = "gagal";
                transaksi.Pembayaran.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = $"Pembayaran {transaksi.KodeTransaksi} ditandai Gagal/Ditolak.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (transaksi.Pembayaran.StatusPembayaran == "lunas")
            {
                TempData["Success"] = $"Pembayaran {transaksi.KodeTransaksi} sudah LUNAS sebelumnya.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (transaksi.StatusTransaksi == "dibatalkan")
            {
                TempData["Error"] = "Transaksi sudah dibatalkan, pembayaran tidak dapat diverifikasi.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Validasi & potong stok (FR-19 & BR-06)
            foreach (var d in transaksi.DetailTransaksis)
            {
                if (d.Produk == null || d.Produk.Stok < d.Jumlah)
                {
                    TempData["Error"] = $"Stok '{d.Produk?.NamaProduk ?? "Produk"}' tidak mencukupi (tersisa {d.Produk?.Stok ?? 0}, butuh {d.Jumlah}).";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            foreach (var d in transaksi.DetailTransaksis)
            {
                d.Produk!.Stok -= d.Jumlah;
                d.Produk.UpdatedAt = DateTime.Now;
            }

            transaksi.Pembayaran.StatusPembayaran = "lunas";
            transaksi.Pembayaran.TanggalBayar ??= DateTime.Now;
            transaksi.Pembayaran.UpdatedAt = DateTime.Now;

            if (transaksi.StatusTransaksi == "pending")
            {
                transaksi.StatusTransaksi = "dikonfirmasi";
            }
            transaksi.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = $"Pembayaran {transaksi.KodeTransaksi} diverifikasi LUNAS dan stok produk otomatis dikurangi.";
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            TempData["Error"] = $"Gagal memverifikasi pembayaran: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(ulong id, string statusTransaksi)
    {
        var transaksi = await _db.Transaksis.Include(t => t.Pembayaran).FirstOrDefaultAsync(t => t.IdTransaksi == id);
        if (transaksi == null) return NotFound();

        string[] butuhLunas = ["dikonfirmasi", "diproses", "dikirim", "selesai"];
        if (butuhLunas.Contains(statusTransaksi) && transaksi.Pembayaran?.StatusPembayaran != "lunas")
        {
            TempData["Error"] = "Status pesanan belum bisa dilanjutkan sebelum pembayaran berstatus LUNAS.";
            return RedirectToAction(nameof(Details), new { id });
        }

        transaksi.StatusTransaksi = statusTransaksi;
        transaksi.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Status pesanan {transaksi.KodeTransaksi} diperbarui ke {statusTransaksi.ToUpperInvariant()}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePengiriman(ulong id, string metodePengiriman, string? noResi, string statusPengiriman)
    {
        var transaksi = await _db.Transaksis
            .Include(t => t.Pembayaran)
            .Include(t => t.Pengiriman)
            .FirstOrDefaultAsync(t => t.IdTransaksi == id);

        if (transaksi == null) return NotFound();

        if (statusPengiriman != "menunggu_konfirmasi" && transaksi.Pembayaran?.StatusPembayaran != "lunas")
        {
            TempData["Error"] = "Pengiriman hanya dapat diproses setelah pembayaran diverifikasi LUNAS.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if ((statusPengiriman == "dikirim" || statusPengiriman == "diterima") && string.IsNullOrWhiteSpace(noResi))
        {
            TempData["Error"] = "Nomor resi wajib diisi untuk status Dikirim / Diterima.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (transaksi.Pengiriman == null)
        {
            transaksi.Pengiriman = new Pengiriman
            {
                IdTransaksi = id,
                MetodePengiriman = metodePengiriman.Trim(),
                NoResi = noResi?.Trim(),
                StatusPengiriman = statusPengiriman,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _db.Pengirimans.Add(transaksi.Pengiriman);
        }
        else
        {
            transaksi.Pengiriman.MetodePengiriman = metodePengiriman.Trim();
            transaksi.Pengiriman.NoResi = noResi?.Trim();
            transaksi.Pengiriman.StatusPengiriman = statusPengiriman;
            transaksi.Pengiriman.UpdatedAt = DateTime.Now;
        }

        if (transaksi.StatusTransaksi != "dibatalkan")
        {
            transaksi.StatusTransaksi = statusPengiriman switch
            {
                "dikemas" => "diproses",
                "dikirim" => "dikirim",
                "diterima" => "selesai",
                _ => transaksi.StatusTransaksi
            };
            transaksi.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Data pengiriman {transaksi.KodeTransaksi} berhasil diperbarui.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ulong id)
    {
        var transaksi = await _db.Transaksis.Include(t => t.Pembayaran).FirstOrDefaultAsync(t => t.IdTransaksi == id);
        if (transaksi == null) return NotFound();

        if (transaksi.Pembayaran?.StatusPembayaran == "lunas" && transaksi.StatusTransaksi != "dibatalkan")
        {
            TempData["Error"] = "Transaksi yang sudah LUNAS tidak dapat dihapus.";
            return RedirectToAction(nameof(Index));
        }

        _db.Transaksis.Remove(transaksi);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Transaksi {transaksi.KodeTransaksi} berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Transaksi?> GetFullTransaksi(ulong id)
    {
        return await _db.Transaksis
            .Include(t => t.Pelanggan)
            .Include(t => t.Admin)
            .Include(t => t.Promo)
            .Include(t => t.Pembayaran)
            .Include(t => t.Pengiriman)
            .Include(t => t.DetailTransaksis)
                .ThenInclude(d => d.Produk)
                    .ThenInclude(p => p!.Kategori)
            .FirstOrDefaultAsync(t => t.IdTransaksi == id);
    }

    private async Task LoadPosLists()
    {
        ViewBag.Pelanggans = await _db.Pelanggans.OrderBy(p => p.Nama).ToListAsync();
        ViewBag.Produks = await _db.Produks.Include(p => p.Kategori).Where(p => p.Stok > 0).OrderBy(p => p.NamaProduk).ToListAsync();
        var today = DateTime.Today;
        ViewBag.Promos = await _db.Promos.Where(p => p.TanggalMulai <= today && p.TanggalSelesai >= today).ToListAsync();
    }
}
