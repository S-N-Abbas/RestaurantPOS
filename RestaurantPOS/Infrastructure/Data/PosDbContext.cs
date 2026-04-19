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

            // ─── Booking → Table (optional) ───────────────────────────────────────────
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Table)
                .WithMany()
                .HasForeignKey(b => b.TableId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ─── Booking → Order (optional, set when seated) ──────────────────────────
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

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Drinks", IsActive = true },
                new Category { Id = 2, Name = "Starters", IsActive = true },
                new Category { Id = 3, Name = "Main Course", IsActive = true },
                new Category { Id = 4, Name = "BBQ", IsActive = true },
                new Category { Id = 5, Name = "Biryani & Rice", IsActive = true },
                new Category { Id = 6, Name = "Desserts", IsActive = true }
            );

            modelBuilder.Entity<MenuProduct>().HasData(

    // Drinks
    new MenuProduct { Id = 1, Name = "Masala Chai", Price = 2.50m, CategoryId = 1, IsActive = true },
    new MenuProduct { Id = 2, Name = "Karak Chai", Price = 2.80m, CategoryId = 1, IsActive = true },
    new MenuProduct { Id = 3, Name = "Mango Lassi", Price = 3.50m, CategoryId = 1, IsActive = true },
    new MenuProduct { Id = 4, Name = "Soft Drink (Can)", Price = 1.50m, CategoryId = 1, IsActive = true },
    new MenuProduct { Id = 5, Name = "Fresh Lime Soda", Price = 2.80m, CategoryId = 1, IsActive = true },

    // Starters
    new MenuProduct { Id = 6, Name = "Samosa (2 pcs)", Price = 3.00m, CategoryId = 2, IsActive = true },
    new MenuProduct { Id = 7, Name = "Chicken Pakora", Price = 5.50m, CategoryId = 2, IsActive = true },
    new MenuProduct { Id = 8, Name = "Chana Chaat", Price = 4.50m, CategoryId = 2, IsActive = true },

    // Main Course
    new MenuProduct { Id = 9, Name = "Chicken Karahi", Price = 11.99m, CategoryId = 3, IsActive = true },
    new MenuProduct { Id = 10, Name = "Lamb Karahi", Price = 13.99m, CategoryId = 3, IsActive = true },
    new MenuProduct { Id = 11, Name = "Chicken Curry", Price = 9.99m, CategoryId = 3, IsActive = true },
    new MenuProduct { Id = 12, Name = "Daal Tarka", Price = 7.99m, CategoryId = 3, IsActive = true },
    new MenuProduct { Id = 13, Name = "Paneer Karahi", Price = 9.50m, CategoryId = 3, IsActive = true },

    // BBQ
    new MenuProduct { Id = 14, Name = "Chicken Tikka (Full)", Price = 8.99m, CategoryId = 4, IsActive = true },
    new MenuProduct { Id = 15, Name = "Seekh Kebab (2 pcs)", Price = 7.50m, CategoryId = 4, IsActive = true },
    new MenuProduct { Id = 16, Name = "Mixed Grill", Price = 15.99m, CategoryId = 4, IsActive = true },

    // Biryani & Rice
    new MenuProduct { Id = 17, Name = "Chicken Biryani", Price = 8.99m, CategoryId = 5, IsActive = true },
    new MenuProduct { Id = 18, Name = "Lamb Biryani", Price = 10.99m, CategoryId = 5, IsActive = true },
    new MenuProduct { Id = 19, Name = "Plain Rice", Price = 3.50m, CategoryId = 5, IsActive = true },
    new MenuProduct { Id = 20, Name = "Pilau Rice", Price = 4.00m, CategoryId = 5, IsActive = true },

    // Desserts
    new MenuProduct { Id = 21, Name = "Gulab Jamun (2 pcs)", Price = 3.99m, CategoryId = 6, IsActive = true },
    new MenuProduct { Id = 22, Name = "Kheer", Price = 3.50m, CategoryId = 6, IsActive = true },
    new MenuProduct { Id = 23, Name = "Ras Malai", Price = 4.50m, CategoryId = 6, IsActive = true }
);


            modelBuilder.Entity<Table>().HasData(
                Enumerable.Range(1, 12).Select(i =>
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
