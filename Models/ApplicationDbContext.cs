using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WatchListV2.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApiUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SeriesModel> Series => Set<SeriesModel>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //For ApiUsers
            modelBuilder.Entity<ApiUsers>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).HasColumnName("UserID");
            });

            //For SeriesModel
            modelBuilder.Entity<SeriesModel>(entity =>
            {
                entity.HasKey(k => k.SeriesID); //Primary Key
                entity.Property(p => p.SeriesID).ValueGeneratedOnAdd();//Auto Generate ID
                entity.HasOne(h => h.ApiUsers)
                .WithMany(u => u.Series) //Each User can have multiple series
                .HasForeignKey(f => f.UserID) //Defines UserID as foreign Key
                .OnDelete(DeleteBehavior.Cascade);
                
            });
            //creating RowVersioning for concurrency
            modelBuilder.Entity<SeriesModel>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

        }
    }
}
