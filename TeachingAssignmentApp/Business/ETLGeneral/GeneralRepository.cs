using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Business.ETLGeneral.Model;
using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Business.ETLGeneral
{
    public class GeneralRepository : IGeneralRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        public GeneralRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public virtual async Task<IEnumerable<ETLGenteralResponse>> GetAllAync(ETLGeneralQueryModel model)
        {
            var generals = new List<ETLGenteralResponse> { };

            IEnumerable<Data.ETLGeneral> query = await _context.ETLGenerals
                .Where(x => x.Type == model.Type).ToListAsync();

            foreach (var general in query)
            {
                generals.Add(new ETLGenteralResponse()
                {
                    Label = general.Label,
                    Value = general.Value,
                    Type = general?.Type,
                    Category = general?.Category
                });
            }

            return generals;
        }

        public async Task<IEnumerable<Data.Teacher>> ListAllTeacherAsync()
        {
            return await _context.Teachers.ToListAsync();
        }

        public async Task<IEnumerable<Data.ProfessionalGroup>> ListAllProfessionalGroupAsync()
        {
            return await _context.ProfessionalGroups.ToListAsync();
        }

        public async Task<IEnumerable<Data.Class>> ListAllClassAsync()
        {
            return await _context.Classes.ToListAsync();
        }

        public async Task<IEnumerable<Data.Aspiration>> ListAllAspirationAsync()
        {
            return await _context.Aspirations.ToListAsync();
        }

        public async Task DeleteByTypeAsync(List<string> types)
        {
            IEnumerable<Data.ETLGeneral> etlGenerals = await _context.ETLGenerals.Where(x => types.Contains(x.Type)).ToListAsync();
            if (etlGenerals.Any()) 
            {
                _context.ETLGenerals.RemoveRange(etlGenerals);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Data.ETLGeneral>> SaveAsync(IEnumerable<Data.ETLGeneral> generals)
        {
            var updates = new List<Data.ETLGeneral>();
            foreach (var general in generals)
            {
                if (general.Id != Guid.Empty)
                {
                    general.Id = Guid.NewGuid();
                }
                _context.ETLGenerals.Add(general);
                updates.Add(general);
            }

            var updated = await _context.SaveChangesAsync();

            if (updated <= 0)
            {
                throw new Exception($"General updated failed.");
            }

            return updates;
        }
    }
}
