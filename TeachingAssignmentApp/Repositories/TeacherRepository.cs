using Microsoft.EntityFrameworkCore;
using System;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeachingAssignmentDbContext _context;

        protected string TeacherTableName
        {
            get { return _context.Model.FindEntityType(typeof(Teacher)).GetTableName(); }
        }

        public TeacherRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<Teacher>> GetAllAsync(TeacherQueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Teacher> query = BuildQuery(queryModel);
            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Teacher> GetByIdAsync(Guid id)
        {
            return await _context.Teachers.FindAsync(id);
        }

        public async Task<Teacher> GetByNameAsync(string name)
        {
            return await _context.Teachers.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task AddAsync(Teacher teacher)
        {
            if (teacher.Id == Guid.Empty)
            {
                teacher.Id = Guid.NewGuid();
            }

            await _context.Teachers.AddAsync(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Teacher teacher)
        {
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Teacher> teachers)
        {
            await _context.Teachers.AddRangeAsync(teachers);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Teacher> BuildQuery(TeacherQueryModel queryModel)
        {
            IQueryable<Teacher> query = _context.Teachers;

            if (!string.IsNullOrEmpty(queryModel.Name))
            {
                query = query.Where(x => x.Name == queryModel.Name);
            }

            return query;
        }

        protected string BuildColumnsTeacher()
        {
            return @"
       [Id]
      ,[Name]
      ,[GdTeaching]
      ,[GdInstruct]
      ";
        }
    }
}