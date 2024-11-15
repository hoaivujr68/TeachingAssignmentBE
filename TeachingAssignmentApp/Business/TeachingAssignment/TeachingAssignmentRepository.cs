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
                PageSize = 200
            };

            var allClasses = await _classRepository.GetAllAsync(queryClassModel);

            var assignedClassCodes = await _context.TeachingAssignments
                                                    .Select(t => t.Code)
                                                    .ToListAsync();

            var classesNotAssigned = allClasses.Content
                                                .Where(c => !assignedClassCodes.Contains(c.Code))
                                                .ToList();

            var result = new Pagination<ClassModel>
            {
                Content = classesNotAssigned,
                CurrentPage = queryModel.CurrentPage ?? 1,
                PageSize = queryModel.PageSize ?? 20,
                TotalRecords = classesNotAssigned.Count
            };

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
                //.Include(t => t.ListCourse)
                //.Include(t => t.TeachingAssignmentProfessionalGroups);

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
    }
}
