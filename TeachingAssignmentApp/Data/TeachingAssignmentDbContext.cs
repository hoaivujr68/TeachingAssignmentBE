using Microsoft.EntityFrameworkCore;

namespace TeachingAssignmentApp.Data
{
    public class TeachingAssignmentDbContext : DbContext
    {
        public TeachingAssignmentDbContext(DbContextOptions<TeachingAssignmentDbContext> options) : base(options)
        {
        }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ProfessionalGroup> ProfessionalGroups { get; set; }
        public DbSet<Course> Courses { get; set; }
    }
}
