using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.Entities.Tables
{
    [Table("b_password_reset")]
    public class PasswordReset
    {
        [Column("email")]
        [Key]
        [Required]
        public required string Email { get; set; }

        [Column("reset_code")]
        [Required]
        public required string ResetCode { get; set; }

        [Column("date_expire")]
        [Required]
        public DateTime DateExpire { get; set; }

    }
}
