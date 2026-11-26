using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Data;
using WearDrop.Web.Models;

namespace WearDrop.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel();
        try
        {
            var today = DateTime.Today;

            vm.TotalProduk = await _db.Produks.CountAsync();
            vm.TotalKategori = await _db.Kategoris.CountAsync();
            vm.TotalTransaksi = await _db.Transaksis.CountAsync();
            vm.TotalPelanggan = await _db.Pelanggans.CountAsync();
            vm.TotalStaf = await _db.Admins.CountAsync();

            vm.TotalPendapatan = await _db.Transaksis
                .Where(t => t.Pembayaran != null && t.Pembayaran.StatusPembayaran == "lunas")
                .SumAsync(t => (decimal?)t.TotalPembayaran) ?? 0m;

            vm.MenungguVerifikasi = await _db.Pembayarans
                .CountAsync(p => p.StatusPembayaran == "menunggu_verifikasi");

            vm.ProdukHabis = await _db.Produks.CountAsync(p => p.Stok <= 0);

            vm.PromoAktif = await _db.Promos
                .CountAsync(p => p.TanggalMulai <= today && p.TanggalSelesai >= today);

            vm.TransaksiTerbaru = await _db.Transaksis
                .Include(t => t.Pelanggan)
                .Include(t => t.Pembayaran)
                .OrderByDescending(t => t.TanggalTransaksi)
                .Take(5)
                .ToListAsync();

            vm.StokKritis = await _db.Produks
                .Include(p => p.Kategori)
                .Where(p => p.Stok <= 5)
                .OrderBy(p => p.Stok)
                .Take(5)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            vm.DatabaseConnected = false;
            vm.DatabaseError = ex.Message;
        }

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
