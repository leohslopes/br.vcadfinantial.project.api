using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.Entities.Tables;


namespace br.vcadfinantial.project.domain.Interfaces.Repositories
{
    public interface IDocumentRepository : IBaseRepository<Document>
    {
        Task InactivateDocumentsByMonth();

        Task<IEnumerable<DocumentAccountInfoAgreggate>> GetAll();

        Task<IEnumerable<DocumentAccountInfoAgreggate>> GetByAccountKey(long accountKey);

        Task<IEnumerable<ReportLogInfoAgreggate>> GetReport(string mounthKey);
    }
}
