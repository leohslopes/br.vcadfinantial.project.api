using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.repository.Database;
using Microsoft.EntityFrameworkCore;


namespace br.vcadfinantial.project.repository.Repositories
{
    public class DocumentRepository : BaseRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(AppDbContext context) : base(context)
        {

        }

        public async Task InactivateDocumentsByMonth(int userId)
        {
            var activesDocs = await _context.Document.Where(x => x.Active && x.CreatedByUserId.Equals(userId)).ToListAsync();
            

            if (activesDocs.Count > 0)
            {
                foreach (var activesDoc in activesDocs)
                {
                    activesDoc.Active = false;
                    var activesAccounts = await _context.Account.Where(x => x.Active && x.IdDocument.Equals(activesDoc.IdDocument)).ToListAsync();

                    foreach (var activesAccount in activesAccounts)
                    {
                        activesAccount.Active = false;
                    }

                    _context.Account.UpdateRange(activesAccounts);
                }

                _context.Document.UpdateRange(activesDocs);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<DocumentAccountInfoAgreggate>> GetAll(int userId)
        {
            IEnumerable<DocumentAccountInfoAgreggate> result;

            result = await _context.Account
                         .Where(x => x.Document.Active && x.Active && x.Document.CreatedByUserId.Equals(userId))
                         .Select(y => new DocumentAccountInfoAgreggate
                         {
                             MounthKey = y.Document!.MounthKey,
                             FileName = y.Document.FileName,
                             OfficialNumber = y.Document.OfficialNumber,
                             Among = y.Among,
                             AccountKey = y.AccountKey
                         })
                         .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DocumentAccountInfoAgreggate>> GetByAccountKey(long accountKey, int userId)
        {
            IEnumerable<DocumentAccountInfoAgreggate> result;

            result = await _context.Account
                         .Where(x => x.Document.Active && x.Active && x.Document.CreatedByUserId.Equals(userId) && x.AccountKey.Equals(accountKey))
                         .Select(y => new DocumentAccountInfoAgreggate
                         {
                             MounthKey = y.Document!.MounthKey,
                             FileName = y.Document.FileName,
                             OfficialNumber = y.Document.OfficialNumber,
                             Among = y.Among,
                             AccountKey = y.AccountKey
                         })
                         .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<ReportLogInfoAgreggate>> GetReport(string mounthKey, int userId)
        {
            IEnumerable<ReportLogInfoAgreggate> result;

            result = await _context.Account
                    .Where(x => x.Document.MounthKey.Equals(mounthKey) && x.Document.CreatedByUserId.Equals(userId))
                    .Select(y => new ReportLogInfoAgreggate
                    {
                        MounthKey = y.Document.MounthKey,
                        FileName = y.Document.FileName,
                        OfficialNumber = y.Document.OfficialNumber,
                        Active = y.Document.Active,
                        AccountKey = y.AccountKey,
                        Among = y.Among
                    })
                    .ToListAsync();

            return result;
        }

    }
}