# WearDrop .NET 10 CRUD Suite (Visual Studio Community)

Proyek **ASP.NET Core MVC (.NET 10)** terpisah untuk mengelola seluruh CRUD database **WearDrop** menggunakan C# dan Entity Framework Core (MySQL).

## Cara Membuka di Visual Studio Community

1. Pastikan layanan **MySQL** di Laragon sedang berjalan (`127.0.0.1:3306`, database `weardrop`).
2. Buka **Visual Studio Community**.
3. Klik **Open a project or solution**, lalu pilih file:
   - `D:\WearDrop.DotNet\WearDrop.DotNet.sln` (atau `WearDrop.DotNet.slnx`)
4. Tekan tombol **F5** atau klik tombol hijau **Run (`http`)** di bagian atas Visual Studio untuk menjalankan aplikasi di browser.

## Konfigurasi Database

Pengaturan koneksi MySQL berada di `WearDrop.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=weardrop;User=root;Password=;"
}
```

## Modul CRUD yang Tersedia

- **Dashboard (`HomeController`)**: Ringkasan pendapatan lunas, total produk, transaksi terbaru, dan peringatan stok kritis.
- **Kelola Kategori (`KategoriController`)**: Tambah, lihat, cari, ubah, dan hapus kategori pakaian.
- **Produk & Stok (`ProdukController`)**: CRUD produk lengkap, upload gambar, filter stok kritis/habis, dan update cepat stok.
- **Kelola Promo (`PromoController`)**: CRUD kode promo beserta validasi masa berlaku.
- **Transaksi & Pengiriman (`TransaksiController`)**: Input transaksi baru (POS), verifikasi pembayaran (otomatis mengurangi stok produk dalam transaksi database), update nomor resi pengiriman, dan cetak struk.
- **Data Pelanggan (`PelangganController`)**: CRUD data pelanggan.
- **Cek Keranjang (`KeranjangController`)**: Monitoring, tambah, dan hapus item keranjang pelanggan.
- **Kelola Pengguna / Staf (`PenggunaController`)**: CRUD akun internal (`admin`, `kasir`, `karyawan`) lengkap dengan proteksi akun Admin terakhir.
