using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Class
{
    public class ClassRepository : IClassRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        public ClassRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<ClassModel>> GetAllAsync(ClassQueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Class> query = BuildQuery(queryModel);
            var classeModelsQuery = query.Select(classe => new ClassModel
            {
                Id = classe.Id,
                Name = classe.Name,
                Code = classe.Code,
                CourseName = classe.CourseName,
                MaxEnrol = classe.MaxEnrol,
                GdTeaching = classe.GdTeaching,
                Type = classe.Type,
                GroupName = classe.GroupName,
                TimeTable = classe.TimeTable,
                TimeTableDetail = classe.TimeTableDetail
            });
            var result = await classeModelsQuery.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Data.Class> GetByIdAsync(Guid id)
        {
            return await _context.Classes.FindAsync(id);
        }

        public async Task<Data.Class> GetByNameAsync(string name)
        {
            return await _context.Classes.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task AddAsync(Data.Class classe)
        {
            if (classe.Id == Guid.Empty)
            {
                classe.Id = Guid.NewGuid();
            }

            await _context.Classes.AddAsync(classe);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.Class classe)
        {
            _context.Classes.Update(classe);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var classe = await _context.Classes.FindAsync(id);
            if (classe != null)
            {
                _context.Classes.Remove(classe);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.Class> classes)
        {
            await _context.Classes.AddRangeAsync(classes);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Class> BuildQuery(ClassQueryModel queryModel)
        {
            IQueryable<Data.Class> query = _context.Classes;
                //.Include(t => t.ListCourse)
                //.Include(t => t.ClassProfessionalGroups);

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            return query;
        }
    }
}
