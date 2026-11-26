namespace WearDrop.Web.Models
{
    public class DashboardViewModel
    {
        public bool DatabaseConnected { get; set; } = true;
        public string? DatabaseError { get; set; }

        public int TotalProduk { get; set; }
        public int TotalKategori { get; set; }
        public int TotalTransaksi { get; set; }
        public int TotalPelanggan { get; set; }
        public int TotalStaf { get; set; }

        public decimal TotalPendapatan { get; set; }
        public int MenungguVerifikasi { get; set; }
        public int ProdukHabis { get; set; }
        public int PromoAktif { get; set; }

        public List<Transaksi> TransaksiTerbaru { get; set; } = new();
        public List<Produk> StokKritis { get; set; } = new();
    }
}
