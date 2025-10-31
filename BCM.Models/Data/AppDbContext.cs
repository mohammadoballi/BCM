using BCM.Models.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace BCM.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BusinessCard> BusinessCard { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<int>();

            modelBuilder.Entity<BusinessCard>()
            .Property(u => u.Gender)
            .HasConversion<int>();

            modelBuilder.Entity<BusinessCard>()
            .Property(b => b.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")   
            .ValueGeneratedOnAdd();

        }
    }
}
