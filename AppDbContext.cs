using Microsoft.EntityFrameworkCore;
using VintageCafe.Models;

namespace VintageCafe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ✅ Main Tables
        public DbSet<User> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // ✅ Relationship Configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Order → OrderItem (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- MenuItem → OrderItem (One-to-Many)
            modelBuilder.Entity<MenuItem>()
                .HasMany<OrderItem>()
                .WithOne(oi => oi.MenuItem)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Seed Initial Data (Optional)
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, Name = "Masala Tea", Price = 80, Category = "Beverage", IsSnack = false, ImagePath = "/images/foodsimg/Masala Tea.jpeg" },
                new MenuItem { Id = 2, Name = "Mojito", Price = 160, Category = "Beverage", IsSnack = false, ImagePath = "/images/foodsimg/Mojito.jpeg" },
                new MenuItem { Id = 3, Name = "Latte", Price = 150, Category = "Coffee", IsSnack = false, ImagePath = "/images/foodsimg/Latte.jpeg" },
                new MenuItem { Id = 4, Name = "Espresso", Price = 130, Category = "Coffee", IsSnack = false, ImagePath = "/images/foodsimg/Espresso.jpeg" },
                new MenuItem { Id = 5, Name = "Cappuccino", Price = 140, Category = "Coffee", IsSnack = false, ImagePath = "/images/foodsimg/Cappuccino.jpeg" },
                new MenuItem { Id = 6, Name = "Veg Sandwich", Price = 120, Category = "Snacks", IsSnack = true, ImagePath = "/images/foodsimg/Vegsandwich.jpeg" },
                new MenuItem { Id = 7, Name = "Brownie", Price = 180, Category = "Dessert", IsSnack = true, ImagePath = "/images/foodsimg/Brownie.jpeg" }
            );
        }
    }
}
