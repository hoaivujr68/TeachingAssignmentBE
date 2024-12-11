using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Business.CheckAssignment
{
    public class CheckAssignmentRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        public CheckAssignmentRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }
    }
}
