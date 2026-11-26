using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WearDrop.Web.Models
{
    [Table("kategori")]
    public class Kategori
    {
        [Key]
        [Column("id_kategori")]
        public ulong IdKategori { get; set; }

        [Required(ErrorMessage = "Nama kategori wajib diisi.")]
        [StringLength(255)]
        [Column("nama_kategori")]
        [Display(Name = "Nama Kategori")]
        public string NamaKategori { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Produk> Produks { get; set; } = new List<Produk>();
    }
}
