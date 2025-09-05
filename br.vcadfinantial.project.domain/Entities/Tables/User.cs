using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.Entities.Tables
{
    [Table("b_user")]
    public class User
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column("full_name")]
        [MaxLength(80)]
        [Required]
        public required string FullName { get; set; }

        [Column("gender")]
        [MaxLength(1)]
        [Required]
        public required string Gender { get; set; }

        [Column("email")]
        [MaxLength(100)]
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Column("password")]
        [MaxLength(100)]
        [Required]
        public required string Password { get; set; }

        [Column("photo", TypeName = "MEDIUMBLOB")]
        [Required]
        public byte[]? Photo { get; set; }

        [Column("create_date")]
        [Required]
        public DateTime CreateDate { get; set; }
    }
}
