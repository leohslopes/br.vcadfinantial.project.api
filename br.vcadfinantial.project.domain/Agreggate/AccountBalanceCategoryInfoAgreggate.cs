using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.Agreggate
{
    public class AccountBalanceCategoryInfoAgreggate
    {
        public required string Category { get; set; } 

        public int Count { get; set; }

        public decimal Percentage { get; set; }
    }
}
