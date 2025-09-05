using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.repository.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.vcadfinantial.project.repository.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {

        }

        public async Task<User?> GetByEmail(string email) =>
            await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }
}
