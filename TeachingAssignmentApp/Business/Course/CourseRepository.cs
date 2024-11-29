using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Course
{
    public class CourseRepository : ICourseRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        //protected string CourseTableName
        //{
        //    get { return _context.Model.FindEntityType(typeof(Course)).GetTableName(); }
        //}

        public CourseRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<Data.Course>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Course> query = BuildQuery(queryModel);
            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Data.Course> GetByIdAsync(Guid id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task<Data.Course> GetByNameAsync(string name)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(course => course.Name == name);
        }
        public async Task<IEnumerable<Data.Course>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await _context.Courses
                .Where(course => course.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task AddAsync(Data.Course course)
        {
            if (course.Id == Guid.Empty)
            {
                course.Id = Guid.NewGuid();
            }

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.Course> courses)
        {
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Course> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.Course> query = _context.Courses;

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            return query;
        }

        protected string BuildColumnsCourse()
        {
            return @"
       [Id]
      ,[Name]
      ,[ProfessionalGroupId]
      ,[TeacherId]
      ";
        }
    }
}
