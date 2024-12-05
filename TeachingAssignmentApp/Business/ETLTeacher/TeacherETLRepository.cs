using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Business.ETLGeneral.Model;
using TeachingAssignmentApp.Business.ETLTeacher.Model;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ETLTeacher
{
    public class TeacherETLRepository : ITeacherETLRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        public TeacherETLRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public virtual async Task<IEnumerable<ETLGenteralResponse>> GetAllAync(ETLTeacherQueryModel eTLTeacherQueryModel)
        {
            var generals = new List<ETLGenteralResponse> { };

            IEnumerable<Data.ETLTeacher> query = await _context.ETLTeachers
                .Where(x => x.Role == eTLTeacherQueryModel.Role && x.Type == eTLTeacherQueryModel.Type).ToListAsync();

            foreach (var general in query)
            {
                generals.Add(new ETLGenteralResponse()
                {
                    Label = general.Label,
                    Value = general.Value,
                    Type = general?.Type,
                    Category = general?.Category
                });
            }

            return generals;
        }

        public async Task<Data.Teacher> ListAllTeacherAsync(string role)
        {
            return await _context.Teachers.FirstOrDefaultAsync(t => t.Code == role);
        }
        public async Task<IEnumerable<Data.TeachingAssignment>> ListAllGDRealAsync(string role)
        {
            return await _context.TeachingAssignments.Where(t => t.TeacherCode == role).ToListAsync();
        }

        public async Task<IEnumerable<Data.ProjectAssigment>> ListAllGDProjectRealAsync(string role)
        {
            return await _context.ProjectAssigments.Where(t => t.TeacherCode == role).ToListAsync();
        }

        public async Task<IEnumerable<Data.Project>> ListAllProjectAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task DeleteByTypeAsync(List<string> types, string role)
        {
            IEnumerable<Data.ETLTeacher> etlGenerals = await _context.ETLTeachers.Where(x => x.Role == role && types.Contains(x.Type)).ToListAsync();
            if (etlGenerals.Any())
            {
                _context.ETLTeachers.RemoveRange(etlGenerals);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Data.ETLTeacher>> SaveAsync(IEnumerable<Data.ETLTeacher> generals, string role)
        {
            var updates = new List<Data.ETLTeacher>();
            foreach (var general in generals)
            {
                general.Id = Guid.NewGuid();
                general.Role = role;
                _context.ETLTeachers.Add(general);
                updates.Add(general);
            }

            var updated = await _context.SaveChangesAsync();

            if (updated <= 0)
            {
                throw new Exception($"General updated failed.");
            }

            return updates;
        }
    }
}
