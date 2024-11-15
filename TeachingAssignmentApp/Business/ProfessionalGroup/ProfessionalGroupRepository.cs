﻿using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProfessionalGroup
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
            await _context.ProfessionalGroups.AddRangeAsync(professionalGroups);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.ProfessionalGroup> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.ProfessionalGroup> query = _context.ProfessionalGroups
                .Include(pg => pg.ListCourse);

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
