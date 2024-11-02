using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
    }
}
