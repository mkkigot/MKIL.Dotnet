using Microsoft.EntityFrameworkCore;
using MKIL.DotnetTest.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.OrderService.Infrastructure.Data
{
    // Data/OrderDbContext.cs
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<UserCache> UserCaches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(e => e.CreatedDate)
                    .IsRequired();

            });

            // Configure UserCache entity
            modelBuilder.Entity<UserCache>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.SyncedAt)
                    .IsRequired();
            });
        }
    }
}
