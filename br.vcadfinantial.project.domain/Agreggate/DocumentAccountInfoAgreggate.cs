using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.Agreggate
{
    public class DocumentAccountInfoAgreggate
    {
        public required string MounthKey { get; set; }

        public required string FileName { get; set; }

        public required string OfficialNumber { get; set; }

        public decimal Among { get; set; }

        public long AccountKey { get; set; }
    }
}
