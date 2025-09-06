using br.vcadfinantial.project.domain.DTO;

namespace br.vcadfinantial.project.domain.Interfaces.Services
{
    public interface IReportLogService
    {
        Task<string> Export(ReportLogDTO dto);
    }
}
