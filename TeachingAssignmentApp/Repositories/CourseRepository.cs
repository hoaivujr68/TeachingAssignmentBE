using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Repositories
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

        public async Task<Pagination<Course>> GetAllAsync(CourseQueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Course> query = BuildQuery(queryModel);
            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Course> GetByIdAsync(Guid id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task AddAsync(Course course)
        {
            if (course.Id == Guid.Empty)
            {
                course.Id = Guid.NewGuid();
            }

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course course)
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

        public async Task AddRangeAsync(IEnumerable<Course> courses)
        {
            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Course> BuildQuery(CourseQueryModel queryModel)
        {
            IQueryable<Course> query = _context.Courses;

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
