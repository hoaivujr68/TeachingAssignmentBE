﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Data
{
    public class TeachingAssignmentDbContext : IdentityDbContext<User>
    {
        public TeachingAssignmentDbContext(DbContextOptions<TeachingAssignmentDbContext> options) : base(options)
        {
        }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ProfessionalGroup> ProfessionalGroups { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<TeacherProfessionalGroup> TeacherProfessionalGroups { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Aspiration> Aspirations { get; set; }
        public DbSet<TeachingAssignment> TeachingAssignments { get; set; }
        public DbSet<ProjectAssigment> ProjectAssigments { get; set; }
        public DbSet<TimeTableModel> TimeTableModels { get; set; }
        public DbSet<ETLGeneral> ETLGenerals { get; set; }
        public DbSet<ETLTeacher> ETLTeachers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<CheckAssignment> CheckAssignments { get; set; }
        public DbSet<CuckooTeachingAssignment> CuckooTeachingAssignments { get; set; }
        public DbSet<CuckooProjectAssignment> CuckooProjectAssignments { get; set; }
        public DbSet<ProjectAssignmentInput> ProjectAssignmentInputs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập mối quan hệ nhiều-nhiều giữa Teacher và ProfessionalGroup thông qua TeacherProfessionalGroup
            modelBuilder.Entity<TeacherProfessionalGroup>()
                .HasKey(tpg => new { tpg.TeacherId, tpg.ProfessionalGroupId });

            modelBuilder.Entity<TeacherProfessionalGroup>()
                .HasOne(tpg => tpg.Teacher)
                .WithMany(t => t.TeacherProfessionalGroups)
                .HasForeignKey(tpg => tpg.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherProfessionalGroup>()
                .HasOne(tpg => tpg.ProfessionalGroup)
                .WithMany(pg => pg.TeacherProfessionalGroups)
                .HasForeignKey(tpg => tpg.ProfessionalGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.ListCourse)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.ProfessionalGroup)
                .WithMany(pg => pg.ListCourse)
                .HasForeignKey(c => c.ProfessionalGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }

}
