using br.vcadfinantial.project.domain.Entities.Tables;
using Microsoft.EntityFrameworkCore;


namespace br.vcadfinantial.project.repository.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Document> Document { get; set; }

        public DbSet<Account> Account { get; set; }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasKey(d => d.DocumentCode);

            modelBuilder.Entity<Document>()
                .HasMany(d => d.Accounts)   
                .WithOne(a => a.Document)   
                .HasForeignKey(a => a.DocumentCode)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasKey(a => a.ID);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Photo)
                .HasColumnType("MEDIUMBLOB");

            base.OnModelCreating(modelBuilder);
        }

    }
}
