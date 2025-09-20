using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace br.vcadfinantial.project.domain.Entities.Tables
{
    [Table("b_account")]
    public class Account
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column("id_document")]
        [Required]
        public int IdDocument { get; set; }

        [Column("account_key")]
        [Required]
        public long AccountKey { get; set; }

        [Column("among")]
        [Required]
        public decimal Among { get; set; }

        [Column("active")]
        [Required]
        public bool Active { get; set; }

        [Column("create_date")]
        [Required]
        public DateTime CreateDate { get; set; }

        [ForeignKey(nameof(IdDocument))]
        public Document Document { get; set; } = null!;
    }
}
