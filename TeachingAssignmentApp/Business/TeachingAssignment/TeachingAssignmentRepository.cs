using LinqKit;
using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.TeachingAssignment
{
    public class TeachingAssignmentRepository : ITeachingAssignmentRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        private readonly IClassRepository _classRepository;

        public TeachingAssignmentRepository(TeachingAssignmentDbContext context, IClassRepository classRepository)
        {
            _context = context;
            _classRepository = classRepository;
        }

        public async Task<Pagination<Data.TeachingAssignment>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.TeachingAssignment> query = BuildQuery(queryModel);

            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Pagination<ClassModel>> GetClassNotAssignmentAsync(QueryModel queryModel)
        {
            var queryClassModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 200,
                ListTextSearch = queryModel.ListTextSearch
            };

            var allClasses = await _classRepository.GetAllAsync(queryClassModel);

            var assignedClassCodes = await _context.TeachingAssignments
                                                    .Select(t => t.Code)
                                                    .ToListAsync();

            var classesNotAssigned = allClasses.Content
                                                .Where(c => !assignedClassCodes.Contains(c.Code))
                                                .ToList();

            var result = new Pagination<ClassModel>(
                classesNotAssigned,
                classesNotAssigned.Count,
                queryModel.CurrentPage ?? 1,
                queryModel.PageSize ?? 20
);

            return result;
        }


        public async Task<Data.TeachingAssignment> GetByIdAsync(Guid id)
        {
            return await _context.TeachingAssignments.FindAsync(id);
        }

        public async Task AddAsync(Data.TeachingAssignment teachingAssignment)
        {
            if (teachingAssignment.Id == Guid.Empty)
            {
                teachingAssignment.Id = Guid.NewGuid();
            }

            await _context.TeachingAssignments.AddAsync(teachingAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.TeachingAssignment teachingAssignment)
        {
            _context.TeachingAssignments.Update(teachingAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var teachingAssignment = await _context.TeachingAssignments.FindAsync(id);
            if (teachingAssignment != null)
            {
                _context.TeachingAssignments.Remove(teachingAssignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.TeachingAssignment> teachingAssignments)
        {
            await _context.TeachingAssignments.AddRangeAsync(teachingAssignments);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.TeachingAssignment> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.TeachingAssignment> query = _context.TeachingAssignments;

            var predicate = PredicateBuilder.New<Data.TeachingAssignment>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.Name.ToLower().Contains(ts.ToLower()) ||
                        p.Code.ToLower().Contains(ts.ToLower()) ||
                        p.TeacherCode.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }
    }
}
