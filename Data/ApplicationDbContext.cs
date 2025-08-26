using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NiRAProject.models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NiRAProject.Data
{
    public class ApplicationDbContext: IdentityDbContext<RRRModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your entities here
        public DbSet<Domain> Domains { get; set; }
        public DbSet<RegistrarApplicationModel> RegistrarApplications { get; set; }
        public DbSet<domainType> DomainTypes { get; set; }
        public DbSet<DomainKey> DomainKeys { get; set; }

        // Modify the model creating method to enforce unique constraints and some other rules
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RegistrarApplicationModel>()
                .HasIndex(u => u.UserName)
                .IsUnique();
            modelBuilder.Entity<RegistrarApplicationModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<domainType>()
                .HasIndex(dt => dt._domainSuffix)
                .IsUnique();
        }
    }
}