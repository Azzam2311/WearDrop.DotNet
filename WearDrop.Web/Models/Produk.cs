using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("produk")]
    public class Produk
    {
        [Key]
        [Column("id_produk")]
        public ulong IdProduk { get; set; }

        [Required(ErrorMessage = "Kategori wajib dipilih.")]
        [Column("id_kategori")]
        [Display(Name = "Kategori")]
        public ulong IdKategori { get; set; }

        [Required(ErrorMessage = "Nama produk wajib diisi.")]
        [StringLength(255)]
        [Column("nama_produk")]
        [Display(Name = "Nama Produk")]
        public string NamaProduk { get; set; } = string.Empty;

        [Required(ErrorMessage = "Harga produk wajib diisi.")]
        [Range(0, 999999999.99, ErrorMessage = "Harga tidak boleh negatif.")]
        [Column("harga", TypeName = "decimal(12,2)")]
        [Display(Name = "Harga (Rp)")]
        public decimal Harga { get; set; }

        [Required(ErrorMessage = "Stok wajib diisi.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok minimal 0.")]
        [Column("stok")]
        [Display(Name = "Stok")]
        public int Stok { get; set; } = 0;

        [Column("deskripsi")]
        [Display(Name = "Deskripsi")]
        public string? Deskripsi { get; set; }

        [Column("gambar")]
        [Display(Name = "Gambar (URL / Path)")]
        public string? Gambar { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdKategori")]
        public virtual Kategori? Kategori { get; set; }

        public virtual ICollection<DetailTransaksi> DetailTransaksis { get; set; } = new List<DetailTransaksi>();
        public virtual ICollection<Keranjang> Keranjangs { get; set; } = new List<Keranjang>();
    }
}
