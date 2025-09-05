using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<bool> CreateUser(UserDTO dto);

        Task<UserSession> GetToken(LoginDTO dto);
    }
}
