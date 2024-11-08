using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Repositories
{
    public class TeacherProfessionalGroupRepository : ITeacherProfessionalGroupRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        public TeacherProfessionalGroupRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeacherProfessionalGroup>> GetAllAsync()
        {
            return await _context.TeacherProfessionalGroups
                .Include(tp => tp.Teacher)
                .Include(tp => tp.ProfessionalGroup)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherProfessionalGroup>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await _context.TeacherProfessionalGroups
                .Include(tp => tp.ProfessionalGroup)
                .Where(tp => tp.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherProfessionalGroup>> GetByProfessionalGroupIdAsync(Guid professionalGroupId)
        {
            return await _context.TeacherProfessionalGroups
                .Include(tp => tp.Teacher)
                .Where(tp => tp.ProfessionalGroupId == professionalGroupId)
                .ToListAsync();
        }

        public async Task AddAsync(TeacherProfessionalGroup teacherProfessionalGroup)
        {
            await _context.TeacherProfessionalGroups.AddAsync(teacherProfessionalGroup);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TeacherProfessionalGroup> teacherProfessionalGroups)
        {
            await _context.TeacherProfessionalGroups.AddRangeAsync(teacherProfessionalGroups);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid teacherId, Guid professionalGroupId)
        {
            var entry = await _context.TeacherProfessionalGroups
                .FirstOrDefaultAsync(tp => tp.TeacherId == teacherId && tp.ProfessionalGroupId == professionalGroupId);

            if (entry != null)
            {
                _context.TeacherProfessionalGroups.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }
    }
}
