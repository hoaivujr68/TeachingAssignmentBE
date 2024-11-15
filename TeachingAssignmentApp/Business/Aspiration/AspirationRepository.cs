using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Aspiration
{
    public class AspirationRepository : IAspirationRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        public AspirationRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<AspirationModel>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Aspiration> query = BuildQuery(queryModel);
            var aspirationModelsQuery = query.Select(aspiration => new AspirationModel
            {
                Id = aspiration.Id,
                TeacherCode = aspiration.TeacherCode,
                TeacherName = aspiration.TeacherName,
                StudentId = aspiration.StudentId,
                StudentName = aspiration.StudentName,
                Topic = aspiration.Topic,
                ClassName = aspiration.ClassName,
                GroupName = aspiration.GroupName,
                Status = aspiration.Status,
                DesireAccept = aspiration.DesireAccept,
                Aspiration1 = aspiration.Aspiration1,
                Aspiration2 = aspiration.Aspiration2,
                Aspiration3 = aspiration.Aspiration3,
                StatusCode = aspiration.StatusCode
                
            });
            var result = await aspirationModelsQuery.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Data.Aspiration> GetByIdAsync(Guid id)
        {
            return await _context.Aspirations.FindAsync(id);
        }

        public async Task<Data.Aspiration> GetByStudentIdAsync(string studentId)
        {
            return await _context.Aspirations.FirstOrDefaultAsync(a => a.StudentId == studentId);
        }

        public async Task AddAsync(Data.Aspiration aspiration)
        {
            if (aspiration.Id == Guid.Empty)
            {
                aspiration.Id = Guid.NewGuid();
            }

            await _context.Aspirations.AddAsync(aspiration);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Data.Aspiration aspiration)
        {
            _context.Aspirations.Update(aspiration);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var aspiration = await _context.Aspirations.FindAsync(id);
            if (aspiration != null)
            {
                _context.Aspirations.Remove(aspiration);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.Aspiration> aspirations)
        {
            await _context.Aspirations.AddRangeAsync(aspirations);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Aspiration> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.Aspiration> query = _context.Aspirations;
                //.Include(t => t.ListCourse)
                //.Include(t => t.AspirationProfessionalGroups);

            return query;
        }
    }
}
