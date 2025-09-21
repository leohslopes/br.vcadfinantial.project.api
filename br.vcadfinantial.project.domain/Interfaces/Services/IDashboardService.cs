using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;


namespace br.vcadfinantial.project.domain.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<IEnumerable<AccountMinMaxInfoAgreggate>> GetAccount(DashboardDTO dto);

        Task<IEnumerable<AccountBalanceCategoryInfoAgreggate>> GetBalance(DashboardDTO dto);
    }
}
