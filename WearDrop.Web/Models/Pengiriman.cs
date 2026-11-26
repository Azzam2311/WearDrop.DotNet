using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("pengiriman")]
    public class Pengiriman
    {
        [Key]
        [Column("id_pengiriman")]
        public ulong IdPengiriman { get; set; }

        [Required]
        [Column("id_transaksi")]
        public ulong IdTransaksi { get; set; }

        [Required]
        [Column("metode_pengiriman")]
        [Display(Name = "Kurir / Metode Pengiriman")]
        public string MetodePengiriman { get; set; } = string.Empty;

        [Column("no_resi")]
        [Display(Name = "Nomor Resi")]
        public string? NoResi { get; set; }

        [Required]
        [Column("status_pengiriman")]
        [Display(Name = "Status Pengiriman")]
        public string StatusPengiriman { get; set; } = "menunggu_konfirmasi"; // menunggu_konfirmasi, dikemas, dikirim, diterima

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdTransaksi")]
        public virtual Transaksi? Transaksi { get; set; }
    }
}
