using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("detail_transaksi")]
    public class DetailTransaksi
    {
        [Key]
        [Column("id_detail")]
        public ulong IdDetail { get; set; }

        [Required]
        [Column("id_transaksi")]
        public ulong IdTransaksi { get; set; }

        [Required]
        [Column("id_produk")]
        public ulong IdProduk { get; set; }

        [Required]
        [Column("jumlah")]
        [Display(Name = "Jumlah")]
        public int Jumlah { get; set; }

        [Required]
        [Column("harga", TypeName = "decimal(12,2)")]
        [Display(Name = "Harga Satuan")]
        public decimal Harga { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(12,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdTransaksi")]
        public virtual Transaksi? Transaksi { get; set; }

        [ForeignKey("IdProduk")]
        public virtual Produk? Produk { get; set; }
    }
}
