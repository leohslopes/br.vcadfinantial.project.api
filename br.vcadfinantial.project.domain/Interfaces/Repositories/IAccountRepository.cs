using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.Entities.Tables;


namespace br.vcadfinantial.project.domain.Interfaces.Repositories
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<IEnumerable<AccountMinMaxInfoAgreggate>> GetAccounts(int userId);

        Task<IEnumerable<AccountBalanceCategoryInfoAgreggate>> GetBalances(int userId);
    }
}
