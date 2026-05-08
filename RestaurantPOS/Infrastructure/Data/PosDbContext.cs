using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Domain;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantPOS.Services;

namespace RestaurantPOS.Infrastructure.Data
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<MenuProduct> Products => Set<MenuProduct>();
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<User> Users => Set<User>();

        public DbSet<Booking> Bookings => Set<Booking>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .IsRequired(false);   // ✅ TakeAway orders have no table

            modelBuilder.Entity<MenuProduct>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .IsRequired(false);   // ✅ null = open item

            // ─── Bookings → Table (optional) ───────────────────────────────────────────
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Table)
                .WithMany()
                .HasForeignKey(b => b.TableId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ─── Bookings → Order (optional, set when seated) ──────────────────────────
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Order)
                .WithMany()
                .HasForeignKey(b => b.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ─── Deposit precision ────────────────────────────────────────────────────
            modelBuilder.Entity<Booking>()
                .Property(b => b.DepositAmount)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "Admin", PasscodeHash = "1234", Role = "Admin" }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Starters", IsActive = true },
                new Category { Id = 2, Name = "Main Course", IsActive = true },
                new Category { Id = 3, Name = "Rice & Bread", IsActive = true },
                new Category { Id = 4, Name = "Desserts", IsActive = true },
                new Category { Id = 5, Name = "Milkshakes", IsActive = true },
                new Category { Id = 6, Name = "Mocktails", IsActive = true },
                new Category { Id = 7, Name = "Daiquiris", IsActive = true }
            );

            // Menu Products - Start from Id = 1
            modelBuilder.Entity<MenuProduct>().HasData(

                // === STARTERS ===
                new MenuProduct { Id = 1, Name = "Samosa (3 pcs)", Price = 3.50m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 2, Name = "Pakora (4 pcs)", Price = 3.00m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 3, Name = "Chicken Pakora (4 pcs)", Price = 4.00m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 4, Name = "Fish Pakora (4 pcs)", Price = 4.00m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 5, Name = "Chapli Kabab (3 pcs)", Price = 4.00m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 6, Name = "Seekh Kabab (4 pcs)", Price = 3.50m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 7, Name = "Drum Stick (3 pcs)", Price = 4.00m, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 8, Name = "Wings (4 pcs)", Price = 4.00m, CategoryId = 1, IsActive = true },

                // === MAIN COURSE ===
                new MenuProduct { Id = 9, Name = "Dal", Price = 6.95m, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 10, Name = "Lamb Karahi", Price = 8.95m, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 11, Name = "Palak Chicken", Price = 7.95m, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 12, Name = "Mince Karahi", Price = 8.95m, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 13, Name = "Butter Chicken", Price = 8.95m, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 14, Name = "Chicken Manchurian", Price = 7.95m, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 15, Name = "Chicken Tikka Karahi", Price = 7.95m, CategoryId = 2, IsActive = true },

                // === RICE & BREAD ===
                new MenuProduct { Id = 16, Name = "Plain Rice", Price = 5.00m, CategoryId = 3, IsActive = true },
                new MenuProduct { Id = 17, Name = "Fried Egg Rice", Price = 5.95m, CategoryId = 3, IsActive = true },
                new MenuProduct { Id = 18, Name = "Pulao Rice", Price = 7.95m, CategoryId = 3, IsActive = true },
                new MenuProduct { Id = 19, Name = "Roti", Price = 0.75m, CategoryId = 3, IsActive = true },
                new MenuProduct { Id = 20, Name = "Naan", Price = 1.50m, CategoryId = 3, IsActive = true },

                // === DESSERTS ===
                new MenuProduct { Id = 21, Name = "Kheer", Price = 6.95m, CategoryId = 4, IsActive = true },
                new MenuProduct { Id = 22, Name = "Custard Trifle", Price = 5.95m, CategoryId = 4, IsActive = true },

                // === MILKSHAKES ===
                new MenuProduct { Id = 23, Name = "GoGo Shake", Price = 4.95m, CategoryId = 5, IsActive = true },
                new MenuProduct { Id = 24, Name = "Millionaire Shake", Price = 4.95m, CategoryId = 5, IsActive = true },
                new MenuProduct { Id = 25, Name = "Hershey’s Shake", Price = 4.95m, CategoryId = 5, IsActive = true },
                new MenuProduct { Id = 26, Name = "Reese’s Peanut Butter Shake", Price = 4.95m, CategoryId = 5, IsActive = true },
                new MenuProduct { Id = 27, Name = "Oreo Shake", Price = 4.95m, CategoryId = 5, IsActive = true },
                new MenuProduct { Id = 28, Name = "Jaffa Cake Shake", Price = 4.95m, CategoryId = 5, IsActive = true },
                new MenuProduct { Id = 29, Name = "Berry Nice Shake", Price = 4.95m, CategoryId = 5, IsActive = true },

                // === MOCKTAILS ===
                new MenuProduct { Id = 30, Name = "Miami Sunset", Price = 4.95m, CategoryId = 6, IsActive = true },
                new MenuProduct { Id = 31, Name = "Strawberry & Mint", Price = 4.95m, CategoryId = 6, IsActive = true },
                new MenuProduct { Id = 32, Name = "Lemon & Lime", Price = 4.95m, CategoryId = 6, IsActive = true },
                new MenuProduct { Id = 33, Name = "Pina Colada Mocktail", Price = 4.95m, CategoryId = 6, IsActive = true },
                new MenuProduct { Id = 34, Name = "Mango Mocktail", Price = 4.95m, CategoryId = 6, IsActive = true },

                // === DAIQUIRIS ===
                new MenuProduct { Id = 35, Name = "Strawberry Daiquiri", Price = 4.95m, CategoryId = 7, IsActive = true },
                new MenuProduct { Id = 36, Name = "Mango Daiquiri", Price = 4.95m, CategoryId = 7, IsActive = true },
                new MenuProduct { Id = 37, Name = "Peach Daiquiri", Price = 4.95m, CategoryId = 7, IsActive = true },
                new MenuProduct { Id = 38, Name = "Blueberry Daiquiri", Price = 4.95m, CategoryId = 7, IsActive = true }
            );

            modelBuilder.Entity<Table>().HasData(
                Enumerable.Range(1, 18).Select(i =>
                    new Table
                    {
                        Id = i,
                        Number = i,
                        IsActive = true
                    }
                )
            );
        }
    }
}
