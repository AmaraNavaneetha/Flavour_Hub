using Microsoft.EntityFrameworkCore;
using restapp.Models;

namespace restapp.Dal
{
    public class RestContext : DbContext
    {
        public RestContext(DbContextOptions<RestContext> options) : base(options)  
        { 

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Sliders>().ToTable("Sliders");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<FoodItem>().ToTable("FoodItem");
            modelBuilder.Entity<ItemType>().ToTable("ItemType");

        }
        public DbSet<Role> roles { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Sliders> sliders { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<ItemType> itemTypes { get; set; }
        public DbSet<FoodItem> fooditems { get; set; }
        
        
        

        public DbSet<restapp.Models.UserLogin> UserLogin { get; set; } = default!;
        public DbSet<restapp.Models.FoodItem> FoodItem { get; set; } = default!;
    }
}
