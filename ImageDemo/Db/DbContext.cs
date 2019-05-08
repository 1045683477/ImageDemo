using ImageDemo.Model;
using Microsoft.EntityFrameworkCore;

namespace ImageDemo.Db
{
    public class DbContext:Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options):base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Images> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Images>()
                .HasKey(x => x.Id);
        }
    }
}
