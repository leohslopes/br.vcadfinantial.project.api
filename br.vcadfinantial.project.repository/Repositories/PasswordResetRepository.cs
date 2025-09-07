using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.repository.Database;


namespace br.vcadfinantial.project.repository.Repositories
{
    public class PasswordResetRepository : BaseRepository<PasswordReset>, IPasswordResetRepository
    {
        public PasswordResetRepository(AppDbContext context) : base(context)
        {

        }
    }
}
