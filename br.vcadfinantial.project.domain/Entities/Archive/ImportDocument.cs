using System.Xml.Serialization;

namespace br.vcadfinantial.project.domain.Entities.Archive
{
    [XmlRoot("documento")]
    public class ImportDocument
    {
        [XmlAttribute("codigoDocumento")]
        public int DocumentCode { get; set; } = default!;

        [XmlAttribute("cnpj")]
        public string OfficialNumber { get; set; } = default!;

        [XmlAttribute("dataBase")]
        public string MounthKey { get; set; } = default!;

        [XmlAttribute("tipoRemessa")]
        public string ShipmentType { get; set; } = default!;

        [XmlArray("contas")]
        [XmlArrayItem("conta")]
        public List<ImportAccount> Accounts { get; set; } = [];
    }
}
