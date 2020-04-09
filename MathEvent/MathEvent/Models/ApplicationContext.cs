using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Section>()
                .HasOne(s => s.Conference)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ConferenceId);

            builder.Entity<Conference>()
                .HasOne(c => c.Manager)
                .WithMany(m => m.Conferences)
                .HasForeignKey(c => c.ManagerId);

            builder.Entity<Section>()
                .HasOne(s => s.Manager)
                .WithMany(m => m.Sections)
                .HasForeignKey(s => s.ManagerId);

            builder.Entity<Performance>()
                .HasOne(p => p.Section)
                .WithMany(s => s.Performances)
                .HasForeignKey(p => p.SectionId);

            builder.Entity<Performance>()
                .HasOne(p => p.Creator)
                .WithMany(c => c.CreatedPerformances)
                .HasForeignKey(p => p.CreatorId);

            builder.Entity<ApplicationUserPerformance>().HasKey(ap => new { ap.ApplicationUserId, ap.PerformanceId });

            builder.Entity<ApplicationUserPerformance>()
                .HasOne(ap => ap.ApplicationUser)
                .WithMany(u => u.ApplicationUserPerformances)
                .HasForeignKey(ap => ap.ApplicationUserId);

            builder.Entity<ApplicationUserPerformance>()
                .HasOne(ap => ap.Performance)
                .WithMany(p => p.ApplicationUserPerformances)
                .HasForeignKey(ap => ap.PerformanceId);
        }

        public DbSet<Performance> Performances { get; set; }
        public DbSet<ApplicationUserPerformance> ApplicationUserPerformances { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Conference> Conferences { get; set; }

    }
}
