using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("pelanggan")]
    public class Pelanggan
    {
        [Key]
        [Column("id_pelanggan")]
        public ulong IdPelanggan { get; set; }

        [Required(ErrorMessage = "Nama pelanggan wajib diisi.")]
        [StringLength(255)]
        [Column("nama")]
        [Display(Name = "Nama Pelanggan")]
        public string Nama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(255)]
        [Column("email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor HP wajib diisi.")]
        [StringLength(20)]
        [Column("no_hp")]
        [Display(Name = "Nomor HP")]
        public string NoHp { get; set; } = string.Empty;

        [Column("alamat")]
        [Display(Name = "Alamat Lengkap")]
        public string? Alamat { get; set; }

        [Column("google_id")]
        public string? GoogleId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
        public virtual ICollection<Keranjang> Keranjangs { get; set; } = new List<Keranjang>();
    }
}
