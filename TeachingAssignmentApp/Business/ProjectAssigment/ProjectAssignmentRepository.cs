using LinqKit;
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
                PageSize = 300,
                ListTextSearch = queryModel.ListTextSearch
            };

            var allProjects = await _aspirationRepository.GetAllAsync(queryProjectModel);

            var assignedProjectCodes = await _context.ProjectAssigments
                                             .Select(t => t.StudentId)
                                             .ToListAsync();

            var aspirationesNotAssigned = allProjects.Content
                                                .Where(c => !assignedProjectCodes.Contains(c.StudentId))
                                                .ToList();

            var result = new Pagination<AspirationModel>(
                aspirationesNotAssigned,
                aspirationesNotAssigned.Count,
                queryModel.CurrentPage ?? 1,
                queryModel.PageSize ?? 20
            );

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

            var predicate = PredicateBuilder.New<Data.ProjectAssigment>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.TeacherCode.ToLower().Contains(ts.ToLower()) ||
                        p.TeacherName.ToLower().Contains(ts.ToLower()) ||
                        p.StudentId.ToLower().Contains(ts.ToLower()) ||
                        p.StudentName.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }
    }
}
