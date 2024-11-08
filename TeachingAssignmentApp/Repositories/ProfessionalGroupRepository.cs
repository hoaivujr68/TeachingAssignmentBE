using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Repositories
{
    public class ProfessionalGroupRepository : IProfessionalGroupRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        //protected string ProfessionalGroupTableName
        //{
        //    get { return _context.Model.FindEntityType(typeof(ProfessionalGroup)).GetTableName(); }
        //}

        public ProfessionalGroupRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<ProfessionalGroup>> GetAllAsync(ProfessionalGroupQueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<ProfessionalGroup> query = BuildQuery(queryModel);
            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<ProfessionalGroup> GetByIdAsync(Guid id)
        {
            return await _context.ProfessionalGroups.FindAsync(id);
        }

        public async Task AddAsync(ProfessionalGroup professionalGroup)
        {
            if (professionalGroup.Id == Guid.Empty)
            {
                professionalGroup.Id = Guid.NewGuid();
            }

            await _context.ProfessionalGroups.AddAsync(professionalGroup);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProfessionalGroup professionalGroup)
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

        public async Task AddRangeAsync(IEnumerable<ProfessionalGroup> professionalGroups)
        {
            await _context.ProfessionalGroups.AddRangeAsync(professionalGroups);
            await _context.SaveChangesAsync();
        }

        private IQueryable<ProfessionalGroup> BuildQuery(ProfessionalGroupQueryModel queryModel)
        {
            IQueryable<ProfessionalGroup> query = _context.ProfessionalGroups;

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            return query;
        }

        protected string BuildColumnsProfessionalGroup()
        {
            return @"
       [Id]
      ,[Name]
      ,[TeacherId]
      ";
        }
    }
}
