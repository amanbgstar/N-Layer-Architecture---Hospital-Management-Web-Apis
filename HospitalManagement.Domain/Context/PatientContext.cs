using HospitalManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Domain.Context
{
    public class PatientContext : DbContext
    {
        public PatientContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<LabTest> LabTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<Patient>()
                .HasMany(t => t.LabTests)
                .WithOne().IsRequired(false);

            modelBuilder.Entity<Patient>(builder =>
            {

                builder.HasKey(p => p.PatientId);
                builder.Property(p => p.PatientId).UseIdentityColumn();
                builder.Property(p => p.PatientFirstName).HasMaxLength(15).IsRequired();
                builder.Property(p => p.PatientLastName).HasMaxLength(15).IsRequired();
                builder.Property(p => p.DateOfBirth).IsRequired();
                builder.Property(p => p.UserName).IsRequired();
                builder.Property(p => p.Gender).IsRequired();
                builder.Property(p => p.BloodGroup).IsRequired(false);
                builder.Property(p => p.Address).IsRequired();
                builder.Property(p => p.City).IsRequired();
                builder.Property(p => p.State).IsRequired();
                builder.Property(p => p.PinCode).IsRequired();
            });

            modelBuilder.Entity<LabTest>(testBuilder =>
            {
                testBuilder.HasKey(l => l.TestId);
                testBuilder.Property(l => l.TestId).UseIdentityColumn();
                testBuilder.Property(l => l.TestName).HasMaxLength(20).IsRequired();
                testBuilder.Property(l => l.TakenDate).IsRequired();
                testBuilder.Property(l => l.ReportGenerationTime).IsRequired();
                testBuilder.Property(l => l.TechnicianName).IsRequired();
                testBuilder.Property(l => l.UserName).IsRequired();
            });

            /*{
                modelBuilder.Entity<Patient>(builder =>
                {

                    builder.HasKey(p => p.PatientId);
                    builder.Property(p => p.PatientId).UseIdentityColumn();
                    builder.Property(p => p.PatientFirstName).HasMaxLength(15).IsRequired();
                    builder.Property(p => p.PatientLastName).HasMaxLength(15).IsRequired();
                    builder.Property(p => p.DateOfBirth).IsRequired();
                    builder.Property(p => p.UserName).IsRequired();
                    builder.Property(p => p.Gender).IsRequired();
                    builder.Property(p => p.BloodGroup).IsRequired(false);
                    builder.Property(p => p.Address).IsRequired();
                    builder.Property(p => p.City).IsRequired();
                    builder.Property(p => p.State).IsRequired();
                    builder.Property(p => p.PinCode).IsRequired();

                    builder.HasMany<LabTest>(l => l.LabTests).
                    WithOne(p => p.Patient).
                    HasForeignKey(t => t.PatientId);

                });

                modelBuilder.Entity<LabTest>(testBuilder =>
                {
                    testBuilder.HasKey(l => l.TestId);
                    testBuilder.Property(l => l.TestId).UseIdentityColumn();
                    testBuilder.Property(l => l.TestName).HasMaxLength(20).IsRequired();
                    testBuilder.Property(l => l.TakenDate).IsRequired();
                    testBuilder.Property(l => l.ReportGenerationTime).IsRequired();
                    testBuilder.Property(l => l.TechnicianName).IsRequired();
                    testBuilder.Property(l => l.UserName).IsRequired();
                });
            }*/
        }
    }
}
