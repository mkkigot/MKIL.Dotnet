using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MKIL.DotnetTest.UserService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.UserService.Infrastructure.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserOrder> UserOrder { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                //if using database: .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.CreatedDate)
                    .IsRequired();
            });

            modelBuilder.Entity<UserOrder>(entity => {
                entity.HasKey(e => e.UId);

                entity.Property(e => e.UId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UserId)
                    .IsRequired();
                
                entity.Property(e => e.OrderId)
                    .IsRequired();
                
                entity.Property(e => e.SyncedAt)
                    .IsRequired();
            });


        }
    }

}
