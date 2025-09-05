using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace br.vcadfinantial.project.domain.Entities.Tables
{
    [Table("b_document")]
    public class Document
    {
        [Column("document_code")]
        [Key]
        public int DocumentCode { get; set; }

        [Column("official_number")]
        [Required]
        [MaxLength(20)]
        public required string OfficialNumber { get; set; }

        [Column("mounth_key")]
        [Required]
        [MaxLength(10)]
        public required string MounthKey { get; set; }

        [Column("shipment_type")]
        [Required]
        [MaxLength(2)]
        public required string ShipmentType { get; set; }

        [Column("file_name")]
        [Required]
        [MaxLength(30)]
        public required string FileName { get; set; }

        [Column("active")]
        [Required]
        public bool Active { get; set; }

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
