using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Project
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        public ProjectRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<ProjectModel>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Project> query = BuildQuery(queryModel);
            var projectModelsQuery = query.Select(project => new ProjectModel
            {
                Id = project.Id,
                Name = project.Name,
                Code = project.Code,
                Type = project.Type,
                CourseName = project.CourseName,
                StudenId = project.StudenId,
                StudentName = project.StudentName,
                GroupName = project.GroupName,
                GdInstruct = project.GdInstruct
            });

            var result = await projectModelsQuery.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Data.Project> GetByIdAsync(Guid id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<Data.Project> GetByNameAsync(string name)
        {
            return await _context.Projects.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Data.Project> GetByStudentIdAsync(string studentId)
        {
            return await _context.Projects.FirstOrDefaultAsync(a => a.StudenId == studentId);
        }

        public async Task<Data.Project> GetByCourseNameAsync(string courseName)
        {
            return await _context.Projects.FirstOrDefaultAsync(t => t.CourseName == courseName);
        }

        public async Task AddAsync(Data.Project project)
        {
            if (project.Id == Guid.Empty)
            {
                project.Id = Guid.NewGuid();
            }

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.Project> projects)
        {
            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Project> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.Project> query = _context.Projects;
                //.Include(t => t.ListCourse)
                //.Include(t => t.ProjectProfessionalGroups);

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            return query;
        }
    }
}

