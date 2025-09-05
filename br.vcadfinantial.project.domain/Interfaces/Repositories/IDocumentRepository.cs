using br.vcadfinantial.project.domain.Entities.Tables;


namespace br.vcadfinantial.project.domain.Interfaces.Repositories
{
    public interface IDocumentRepository : IBaseRepository<Document>
    {
        Task InactivateDocumentsByMonth();
    }
}
