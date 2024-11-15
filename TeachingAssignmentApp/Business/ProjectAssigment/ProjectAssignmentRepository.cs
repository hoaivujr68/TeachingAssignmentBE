using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProjectAssigment
{
    public class ProjectAssignmentRepository : IProjectAssignmentRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        private readonly IAspirationRepository _aspirationRepository;

        public ProjectAssignmentRepository(TeachingAssignmentDbContext context, IAspirationRepository aspirationRepository)
        {
            _context = context;
            _aspirationRepository = aspirationRepository;
        }

        public async Task<Pagination<Data.ProjectAssigment>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.ProjectAssigment> query = BuildQuery(queryModel);

            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Pagination<AspirationModel>> GetProjectNotAssignmentAsync(QueryModel queryModel)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 300
            };

            var allProjects = await _aspirationRepository.GetAllAsync(queryProjectModel);

            var assignedProjectCodes = await _context.ProjectAssigments
                                             .Select(t => t.StudentId)
                                             .ToListAsync();

            var aspirationesNotAssigned = allProjects.Content
                                                .Where(c => !assignedProjectCodes.Contains(c.StudentId))
                                                .ToList();

            var result = new Pagination<AspirationModel>
            {
                Content = aspirationesNotAssigned,
                CurrentPage = queryModel.CurrentPage ?? 1,
                PageSize = queryModel.PageSize ?? 20,
                TotalRecords = aspirationesNotAssigned.Count
            };

            return result;
        }


        public async Task<Data.ProjectAssigment> GetByIdAsync(Guid id)
        {
            return await _context.ProjectAssigments.FindAsync(id);
        }

        public async Task AddAsync(Data.ProjectAssigment projectAssignment)
        {
            if (projectAssignment.Id == Guid.Empty)
            {
                projectAssignment.Id = Guid.NewGuid();
            }

            await _context.ProjectAssigments.AddAsync(projectAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.ProjectAssigment projectAssignment)
        {
            _context.ProjectAssigments.Update(projectAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var projectAssignment = await _context.ProjectAssigments.FindAsync(id);
            if (projectAssignment != null)
            {
                _context.ProjectAssigments.Remove(projectAssignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.ProjectAssigment> projectAssignments)
        {
            await _context.ProjectAssigments.AddRangeAsync(projectAssignments);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.ProjectAssigment> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.ProjectAssigment> query = _context.ProjectAssigments;

            return query;
        }
    }
}
