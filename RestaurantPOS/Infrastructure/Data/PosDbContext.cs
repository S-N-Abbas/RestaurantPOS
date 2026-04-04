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

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Drinks", IsActive = true },
                new Category { Id = 2, Name = "Food", IsActive = true }
            );

            modelBuilder.Entity<MenuProduct>().HasData(
                new MenuProduct { Id = 1, Name = "Tea", Price = 150, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 2, Name = "Coffee", Price = 200, CategoryId = 1, IsActive = true },
                new MenuProduct { Id = 3, Name = "Burger", Price = 550, CategoryId = 2, IsActive = true },
                new MenuProduct { Id = 4, Name = "Pizza", Price = 900, CategoryId = 2, IsActive = true }
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
