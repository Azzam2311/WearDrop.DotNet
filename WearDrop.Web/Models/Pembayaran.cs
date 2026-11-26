using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("pembayaran")]
    public class Pembayaran
    {
        [Key]
        [Column("id_pembayaran")]
        public ulong IdPembayaran { get; set; }

        [Required]
        [Column("id_transaksi")]
        public ulong IdTransaksi { get; set; }

        [Required]
        [Column("metode_pembayaran")]
        [Display(Name = "Metode Pembayaran")]
        public string MetodePembayaran { get; set; } = string.Empty;

        [Column("tanggal_bayar")]
        [Display(Name = "Tanggal Bayar")]
        public DateTime? TanggalBayar { get; set; }

        [Required]
        [Column("status_pembayaran")]
        [Display(Name = "Status Pembayaran")]
        public string StatusPembayaran { get; set; } = "menunggu_verifikasi"; // menunggu_verifikasi, lunas, gagal

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdTransaksi")]
        public virtual Transaksi? Transaksi { get; set; }
    }
}
