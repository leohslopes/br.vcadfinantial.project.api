using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Archive;
using Microsoft.AspNetCore.Http;


namespace br.vcadfinantial.project.domain.Interfaces.Services
{
    public interface IAccountService
    {
        Task<ResultSetImportArchive> ImportFile(DocumentDTO dto);

        Task<IEnumerable<DocumentAccountInfoAgreggate>> GetAll();

        Task<IEnumerable<DocumentAccountInfoAgreggate>> GetByAccountKey(AccountDTO dto);
    }
}
