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

        public async Task InactivateDocumentsByMonth()
        {
            var activesDocs = await _context.Document.Where(x => x.Active).ToListAsync();

            if (activesDocs.Count > 0)
            {
                foreach (var activesDoc in activesDocs)
                {
                    activesDoc.Active = false;
                }

                _context.Document.UpdateRange(activesDocs);

                await _context.SaveChangesAsync();
            }
        }

    }
}