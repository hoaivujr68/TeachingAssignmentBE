using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;
using LinqKit;

namespace TeachingAssignmentApp.Business.Teacher
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        public TeacherRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel, string? role = "giangvien")
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Teacher> query = BuildQuery(queryModel, role);
            var teacherModelsQuery = query.Select(teacher => new TeacherModel
            {
                Id = teacher.Id,
                Code = teacher.Code,
                Name = teacher.Name,
                GdInstruct = teacher.GdInstruct,
                GdTeaching = teacher.GdTeaching,
                ProfessionalGroup= teacher.TeacherProfessionalGroups.Select(tpg => new ProfessionalGroupModel
                {
                    Id = tpg.ProfessionalGroup.Id,
                    Name = tpg.ProfessionalGroup.Name,
                    ListCourse = tpg.ProfessionalGroup.ListCourse.Where(c => c.TeacherId == teacher.Id).Select(c => new CourseModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList()

                }).ToList()
            });
            var result = await teacherModelsQuery.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Data.Teacher> GetByIdAsync(Guid id)
        {
            return await _context.Teachers.FindAsync(id);
        }

        public async Task<Data.Teacher> GetByCodeAsync(string code)
        {
            return await _context.Teachers.FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<Data.Teacher> GetByNameAsync(string name)
        {
            return await _context.Teachers.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task AddAsync(Data.Teacher teacher)
        {
            if (teacher.Id == Guid.Empty)
            {
                teacher.Id = Guid.NewGuid();
            }

            await _context.Teachers.AddAsync(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.Teacher teacher)
        {
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.Teacher> teachers)
        {
            var existingCourses = await _context.Courses.ToListAsync();
            _context.Courses.RemoveRange(existingCourses);
            var existingTeacherProfessionalGroups = await _context.TeacherProfessionalGroups.ToListAsync();
            _context.TeacherProfessionalGroups.RemoveRange(existingTeacherProfessionalGroups);
            var existingTeachers = await _context.Teachers.ToListAsync();
            _context.Teachers.RemoveRange(existingTeachers);
            await _context.Teachers.AddRangeAsync(teachers);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Data.Teacher> teachers)
        {
            _context.Teachers.UpdateRange(teachers);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Teacher> BuildQuery(QueryModel queryModel, string role)
        {
            IQueryable<Data.Teacher> query;

            // Kiểm tra role
            if (role == "lanhdao" || role == "admin")
            {
                query = _context.Teachers.Include(t => t.ListCourse)
                .Include(t => t.TeacherProfessionalGroups);
            }
            else
            {
                // Lọc theo teacherCode khi role không phải giangvien hoặc admin
                query = _context.Teachers.Where(p => p.Code == role).Include(t => t.ListCourse)
                .Include(t => t.TeacherProfessionalGroups);
            }

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            if (!string.IsNullOrEmpty(queryModel.Code))
            {
                query = query.Where(x => x.Code == queryModel.Code);
            }
            var predicate = PredicateBuilder.New<Data.Teacher>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.Name.ToLower().Contains(ts.ToLower()) ||
                        p.Code.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }
    }
}