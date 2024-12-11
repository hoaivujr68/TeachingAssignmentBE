using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
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

        public async Task<Pagination<ClassModel>> GetAllAsync(QueryModel queryModel, string? role = "giangvien")
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Class> query = BuildQuery(queryModel, role);
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
        public async Task<Data.Class> GetByCodeAsync(string code)
        {
            return await _context.Classes.FirstOrDefaultAsync(t => t.Code == code);
        }

        public async Task<List<string>> GetByCourseNameAsync(string courseName)
        {
            return await _context.Classes
                .Where(t => t.CourseName == courseName)
                .Select(t => t.Code)
                .ToListAsync();
        }

        public async Task<double> GetTotalGdTeachingAsync()
        {
            return await _context.Classes.SumAsync(t => t.GdTeaching);
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
            var timeTableModels = await _context.TimeTableModels.ToListAsync();
            _context.TimeTableModels.RemoveRange(timeTableModels);
            var classesRemove = await _context.Classes.ToListAsync();
            _context.Classes.RemoveRange(classesRemove);
            await _context.Classes.AddRangeAsync(classes);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.Class> BuildQuery(QueryModel queryModel, string? role = "giangvien")
        {
            IQueryable<Data.Class> query;

            // Kiểm tra role
            if (role == "lanhdao" || role == "admin")
            {
                query = _context.Classes;
            }
            else
            {
                // Lọc danh sách Class qua liên kết với Teacher có Code = role
                query = _context.Classes.Where(c =>
                    _context.Teachers
                        .Where(t => t.Code == role)
                        .SelectMany(t => t.ListCourse)
                        .Any(courseName => courseName.Name == c.CourseName)
                );

            }

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            if (!string.IsNullOrEmpty(queryModel.Code))
            {
                query = query.Where(x => x.Code == queryModel.Code);
            }
            var predicate = PredicateBuilder.New<Data.Class>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.Name.ToLower().Contains(ts.ToLower()) ||
                        p.Code.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }
    }
}
