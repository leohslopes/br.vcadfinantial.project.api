using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.repository.Database;
using Microsoft.EntityFrameworkCore;


namespace br.vcadfinantial.project.repository.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<AccountMinMaxInfoAgreggate>> GetAccounts(int userId)
        {
            IEnumerable<AccountMinMaxInfoAgreggate> result;

            var max = _context.Account
                      .Include(x => x.Document)
                      .Where(x => x.Document.Active && x.Active && x.Among != 0 && x.Document.CreatedByUserId.Equals(userId))
                      .OrderByDescending(x => x.Among)
                      .Select(y => new AccountMinMaxInfoAgreggate
                      {
                        MounthKey = y.Document.MounthKey,
                        AccountKey = y.AccountKey,
                        Among = y.Among
                      })
                      .Take(1);

            var min = _context.Account
                      .Include(x => x.Document)
                       .Where(x => x.Document.Active == true && x.Among != 0)
                      .OrderBy(x => x.Among)
                      .Select(y => new AccountMinMaxInfoAgreggate
                      {
                        MounthKey = y.Document.MounthKey,
                        AccountKey = y.AccountKey,
                        Among = y.Among
                      })
                      .Take(1);

             result = await max.Union(min).ToListAsync();

            return result;
        }

        public async Task<IEnumerable<AccountBalanceCategoryInfoAgreggate>> GetBalances(int userId)
        {
            IEnumerable<AccountBalanceCategoryInfoAgreggate> result;

            var totalCount = await _context.Account.CountAsync();
            var grouped = await _context.Account
                         .Where(x => x.Document.Active && x.Active && x.Among != 0 && x.Document.CreatedByUserId.Equals(userId))
                         .GroupBy(y => y.Among == 0 ? "Igual a Zero" :
                                  y.Among > 0 ? "Maior que Zero" :
                                  "Menor que Zero")
                         .Select(z => new
                         {
                            Category = z.Key,
                            Count = z.Count()
                         })
                         .ToListAsync();

            result = grouped.Select(g => new AccountBalanceCategoryInfoAgreggate
            {
                Category = g.Category,
                Count = g.Count,
                Percentage = totalCount == 0 ? 0 : (decimal)g.Count * 100 / totalCount
            });

            return result;
        }
    }
}
