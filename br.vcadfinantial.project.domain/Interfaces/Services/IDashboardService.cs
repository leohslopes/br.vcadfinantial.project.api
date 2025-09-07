using br.vcadfinantial.project.domain.Agreggate;


namespace br.vcadfinantial.project.domain.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<IEnumerable<AccountMinMaxInfoAgreggate>> GetAccount();

        Task<IEnumerable<AccountBalanceCategoryInfoAgreggate>> GetBalance();
    }
}
