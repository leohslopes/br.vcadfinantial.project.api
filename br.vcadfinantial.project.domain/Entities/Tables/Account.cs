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

        [Column("document_code")]
        [Required]
        public int DocumentCode { get; set; }

        [Column("account_key")]
        [Required]
        public long AccountKey { get; set; }

        [Column("among")]
        [Required]
        public decimal Among { get; set; }

        [ForeignKey(nameof(DocumentCode))]
        public Document Document { get; set; } = null!;
    }
}
