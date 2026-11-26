using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("admin")]
    public class AdminUser
    {
        [Key]
        [Column("id_admin")]
        public ulong IdAdmin { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100)]
        [Column("nama")]
        [Display(Name = "Nama Lengkap")]
        public string Nama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(255)]
        [Column("email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role / jabatan wajib dipilih.")]
        [Column("role")]
        [Display(Name = "Jabatan")]
        public string Role { get; set; } = "admin"; // admin, kasir, karyawan

        [Column("google_id")]
        public string? GoogleId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
    }
}
