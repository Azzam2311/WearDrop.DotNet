using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("keranjang")]
    public class Keranjang
    {
        [Key]
        [Column("id_keranjang")]
        public ulong IdKeranjang { get; set; }

        [Required]
        [Column("id_pelanggan")]
        [Display(Name = "Pelanggan")]
        public ulong IdPelanggan { get; set; }

        [Required]
        [Column("id_produk")]
        [Display(Name = "Produk")]
        public ulong IdProduk { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Jumlah minimal 1.")]
        [Column("jumlah")]
        [Display(Name = "Jumlah")]
        public int Jumlah { get; set; } = 1;

        [Column("tanggal_tambah")]
        [Display(Name = "Tanggal Ditambahkan")]
        public DateTime TanggalTambah { get; set; } = DateTime.Now;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdPelanggan")]
        public virtual Pelanggan? Pelanggan { get; set; }

        [ForeignKey("IdProduk")]
        public virtual Produk? Produk { get; set; }
    }
}
