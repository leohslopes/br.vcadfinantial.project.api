using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace br.vcadfinantial.project.domain.Entities.Archive
{
    public class ImportAccount
    {
        [XmlAttribute("codigoConta")]
        public long AccountKey { get; set; }

        [XmlAttribute("saldo")]
        public decimal Among { get; set; }
    }
}
