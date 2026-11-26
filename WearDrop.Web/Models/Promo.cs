using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("promo")]
    public class Promo
    {
        [Key]
        [Column("id_promo")]
        public ulong IdPromo { get; set; }

        [Required(ErrorMessage = "Kode promo wajib diisi.")]
        [StringLength(255)]
        [Column("kode_promo")]
        [Display(Name = "Kode Promo")]
        public string KodePromo { get; set; } = string.Empty;

        [Column("deskripsi")]
        [Display(Name = "Deskripsi")]
        public string? Deskripsi { get; set; }

        [Required(ErrorMessage = "Nominal diskon wajib diisi.")]
        [Range(0, 999999999.99, ErrorMessage = "Nominal diskon tidak boleh negatif.")]
        [Column("diskon", TypeName = "decimal(12,2)")]
        [Display(Name = "Diskon (Rp)")]
        public decimal Diskon { get; set; }

        [Required(ErrorMessage = "Tanggal mulai wajib diisi.")]
        [Column("tanggal_mulai", TypeName = "date")]
        [DataType(DataType.Date)]
        [Display(Name = "Tanggal Mulai")]
        public DateTime TanggalMulai { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Tanggal selesai wajib diisi.")]
        [Column("tanggal_selesai", TypeName = "date")]
        [DataType(DataType.Date)]
        [Display(Name = "Tanggal Selesai")]
        public DateTime TanggalSelesai { get; set; } = DateTime.Today.AddDays(7);

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public bool IsAktif => DateTime.Today >= TanggalMulai.Date && DateTime.Today <= TanggalSelesai.Date;

        public virtual ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
    }
}
