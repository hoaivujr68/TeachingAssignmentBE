using Microsoft.EntityFrameworkCore;
using System;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Teacher
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        protected string TeacherTableName
        {
            get { return _context.Model.FindEntityType(typeof(Data.Teacher)).GetTableName(); }
        }

        public TeacherRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Teacher> query = BuildQuery(queryModel);
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
            await _context.Teachers.AddRangeAsync(teachers);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Teacher> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.Teacher> query = _context.Teachers
                .Include(t => t.ListCourse)
                .Include(t => t.TeacherProfessionalGroups);

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            if (!string.IsNullOrEmpty(queryModel.Code))
            {
                query = query.Where(x => x.Code == queryModel.Code);
            }

            return query;
        }

        protected string BuildColumnsTeacher()
        {
            return @"
       [Id]
      ,[Name]
      ,[GdTeaching]
      ,[GdInstruct]
      ";
        }
    }
}