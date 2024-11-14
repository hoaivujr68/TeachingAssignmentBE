﻿using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Class
{
    public interface IClassRepository
    {
        Task<Pagination<ClassModel>> GetAllAsync(ClassQueryModel queryModel);
        Task<Data.Class> GetByIdAsync(Guid id);
        Task<Data.Class> GetByNameAsync(string name);
        Task AddAsync(Data.Class classe);
        Task UpdateAsync(Data.Class classe);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Class> classes);
    }
}