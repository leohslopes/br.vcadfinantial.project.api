using br.vcadfinantial.project.domain.Agreggate;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.domain.Interfaces.Services;
using br.vcadfinantial.project.repository.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly IAccountRepository _accountRepository;
       
        public DashboardService(ILogger<DashboardService> logger, IAccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<AccountMinMaxInfoAgreggate>> GetAccount(DashboardDTO dto)
        {
            IEnumerable<AccountMinMaxInfoAgreggate> result;

            try
            {
                result = await _accountRepository.GetAccounts(dto.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetAccount] - Erro ao consultar o valor minímo e máximo de saldo: {ex.Message}");
                throw new Exception();
            }

            return result;
        }

        public async Task<IEnumerable<AccountBalanceCategoryInfoAgreggate>> GetBalance(DashboardDTO dto)
        {
            IEnumerable<AccountBalanceCategoryInfoAgreggate> result;

            try
            {
                result = await _accountRepository.GetBalances(dto.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GetBalance] - Erro ao consultar a contagem dos saldos: {ex.Message}");
                throw new Exception();
            }

            return result;
        }
    }
}
