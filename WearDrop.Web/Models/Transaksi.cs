using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("transaksi")]
    public class Transaksi
    {
        [Key]
        [Column("id_transaksi")]
        public ulong IdTransaksi { get; set; }

        [Required]
        [Column("id_pelanggan")]
        [Display(Name = "Pelanggan")]
        public ulong IdPelanggan { get; set; }

        [Column("id_admin")]
        [Display(Name = "Admin / Kasir")]
        public ulong? IdAdmin { get; set; }

        [Column("id_promo")]
        [Display(Name = "Promo")]
        public ulong? IdPromo { get; set; }

        [Column("tanggal_transaksi")]
        [Display(Name = "Tanggal Transaksi")]
        public DateTime TanggalTransaksi { get; set; } = DateTime.Now;

        [Column("total_pembayaran", TypeName = "decimal(12,2)")]
        [Display(Name = "Total Pembayaran (Rp)")]
        public decimal TotalPembayaran { get; set; }

        [Column("status_transaksi")]
        [Display(Name = "Status Transaksi")]
        public string StatusTransaksi { get; set; } = "pending"; // pending, dikonfirmasi, diproses, dikirim, selesai, dibatalkan

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string KodeTransaksi => $"WD-{TanggalTransaksi:yyyyMMdd}-{IdTransaksi:D4}";

        [ForeignKey("IdPelanggan")]
        public virtual Pelanggan? Pelanggan { get; set; }

        [ForeignKey("IdAdmin")]
        public virtual AdminUser? Admin { get; set; }

        [ForeignKey("IdPromo")]
        public virtual Promo? Promo { get; set; }

        public virtual ICollection<DetailTransaksi> DetailTransaksis { get; set; } = new List<DetailTransaksi>();
        public virtual Pembayaran? Pembayaran { get; set; }
        public virtual Pengiriman? Pengiriman { get; set; }
    }
}
