using LinqKit;
using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProfessionalGroup
{
    public class ProfessionalGroupRepository : IProfessionalGroupRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        public ProfessionalGroupRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<ProfessionalGroupModel>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.ProfessionalGroup> query = BuildQuery(queryModel);
            var ProfessionalGroupQuery = query.Select(professionalGroup => new ProfessionalGroupModel
            {
                Id = professionalGroup.Id,
                Name = professionalGroup.Name,
                ListCourse = professionalGroup.ListCourse.Select(c => new CourseModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            });

            var result = await ProfessionalGroupQuery.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Data.ProfessionalGroup> GetByIdAsync(Guid id)
        {
            return await _context.ProfessionalGroups.FindAsync(id);
        }

        public async Task<Data.ProfessionalGroup> GetByNameAsync(string professionalGroupName)
        {
            return await _context.ProfessionalGroups.FindAsync(professionalGroupName);
        }

        public async Task AddAsync(Data.ProfessionalGroup professionalGroup)
        {
            if (professionalGroup.Id == Guid.Empty)
            {
                professionalGroup.Id = Guid.NewGuid();
            }

            await _context.ProfessionalGroups.AddAsync(professionalGroup);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.ProfessionalGroup professionalGroup)
        {
            _context.ProfessionalGroups.Update(professionalGroup);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var professionalGroup = await _context.ProfessionalGroups.FindAsync(id);
            if (professionalGroup != null)
            {
                _context.ProfessionalGroups.Remove(professionalGroup);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.ProfessionalGroup> professionalGroups)
        {
            var existingProfessionalGroups = await _context.ProfessionalGroups.ToListAsync();
            _context.ProfessionalGroups.RemoveRange(existingProfessionalGroups);
            await _context.ProfessionalGroups.AddRangeAsync(professionalGroups);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.ProfessionalGroup> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.ProfessionalGroup> query = _context.ProfessionalGroups
                .Include(pg => pg.ListCourse);

            var predicate = PredicateBuilder.New<Data.ProfessionalGroup>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.Name.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }
            return query;
        }
    }
}
