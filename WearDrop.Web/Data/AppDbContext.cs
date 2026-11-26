using Microsoft.EntityFrameworkCore;
using WearDrop.Web.Models;

namespace WearDrop.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Kategori> Kategoris { get; set; } = null!;
        public DbSet<Produk> Produks { get; set; } = null!;
        public DbSet<Promo> Promos { get; set; } = null!;
        public DbSet<AdminUser> Admins { get; set; } = null!;
        public DbSet<Pelanggan> Pelanggans { get; set; } = null!;
        public DbSet<Keranjang> Keranjangs { get; set; } = null!;
        public DbSet<Transaksi> Transaksis { get; set; } = null!;
        public DbSet<DetailTransaksi> DetailTransaksis { get; set; } = null!;
        public DbSet<Pembayaran> Pembayarans { get; set; } = null!;
        public DbSet<Pengiriman> Pengirimans { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Kategori>().ToTable("kategori");
            modelBuilder.Entity<Produk>().ToTable("produk");
            modelBuilder.Entity<Promo>().ToTable("promo");
            modelBuilder.Entity<AdminUser>().ToTable("admin");
            modelBuilder.Entity<Pelanggan>().ToTable("pelanggan");
            modelBuilder.Entity<Keranjang>().ToTable("keranjang");
            modelBuilder.Entity<Transaksi>().ToTable("transaksi");
            modelBuilder.Entity<DetailTransaksi>().ToTable("detail_transaksi");
            modelBuilder.Entity<Pembayaran>().ToTable("pembayaran");
            modelBuilder.Entity<Pengiriman>().ToTable("pengiriman");

            modelBuilder.Entity<Produk>()
                .HasOne(p => p.Kategori)
                .WithMany(k => k.Produks)
                .HasForeignKey(p => p.IdKategori)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Keranjang>()
                .HasOne(k => k.Pelanggan)
                .WithMany(p => p.Keranjangs)
                .HasForeignKey(k => k.IdPelanggan)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Keranjang>()
                .HasOne(k => k.Produk)
                .WithMany(p => p.Keranjangs)
                .HasForeignKey(k => k.IdProduk)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaksi>()
                .HasOne(t => t.Pelanggan)
                .WithMany(p => p.Transaksis)
                .HasForeignKey(t => t.IdPelanggan);

            modelBuilder.Entity<Transaksi>()
                .HasOne(t => t.Admin)
                .WithMany(a => a.Transaksis)
                .HasForeignKey(t => t.IdAdmin);

            modelBuilder.Entity<Transaksi>()
                .HasOne(t => t.Promo)
                .WithMany(pr => pr.Transaksis)
                .HasForeignKey(t => t.IdPromo);

            modelBuilder.Entity<DetailTransaksi>()
                .HasOne(d => d.Transaksi)
                .WithMany(t => t.DetailTransaksis)
                .HasForeignKey(d => d.IdTransaksi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetailTransaksi>()
                .HasOne(d => d.Produk)
                .WithMany(p => p.DetailTransaksis)
                .HasForeignKey(d => d.IdProduk);

            modelBuilder.Entity<Pembayaran>()
                .HasOne(p => p.Transaksi)
                .WithOne(t => t.Pembayaran)
                .HasForeignKey<Pembayaran>(p => p.IdTransaksi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pengiriman>()
                .HasOne(p => p.Transaksi)
                .WithOne(t => t.Pengiriman)
                .HasForeignKey<Pengiriman>(p => p.IdTransaksi)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
