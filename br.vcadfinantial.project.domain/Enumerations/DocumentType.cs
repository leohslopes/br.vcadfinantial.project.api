using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.Enumerations
{
    public sealed class DocumentType : Enumeration
    {
        public static readonly DocumentType GeneralAccount = new(4010, "Layout Geral");
      
        private DocumentType(int id, string name) : base(id, name) { }

        public static IEnumerable<DocumentType> List() => [GeneralAccount];

        public static DocumentType FromId(int id) =>
            List().FirstOrDefault(d => d.Id == id)
            ?? throw new ArgumentException($"Tipo de documento inválido: {id}");
    }
}
