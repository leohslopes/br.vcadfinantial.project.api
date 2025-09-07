using br.vcadfinantial.project.application.Services;
using br.vcadfinantial.project.domain.DTO;
using br.vcadfinantial.project.domain.Entities.Tables;
using br.vcadfinantial.project.domain.Interfaces.Repositories;
using br.vcadfinantial.project.domain.Interfaces.Services;
using br.vcadfinantial.project.repository.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace br.vcadfinantial.project.crossCutting.IoC
{
    public static class DependecyInjectionBootStrapper
    {
        public static void RegisterAllClasses(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterRepositories(services);
            RegisterServices(services);
        }


        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHasher<UserDTO>, PasswordHasher<UserDTO>>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IReportLogService, ReportLogService>();
            services.AddScoped<IDashboardService, DashboardService>();
        }
    }
}
