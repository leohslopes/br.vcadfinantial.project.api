using br.vcadfinantial.project.domain.Entities.Tables;

namespace br.vcadfinantial.project.domain.Entities.Login
{
    public class UserSession
    {
        public string? Token { get; set; }

        public User? User { get; set; }
    }
}
