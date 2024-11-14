﻿using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Aspiration
{
    public interface IAspirationRepository
    {
        Task<Pagination<AspirationModel>> GetAllAsync(AspirationQueryModel queryModel);
        Task<Data.Aspiration> GetByIdAsync(Guid id);
        Task AddAsync(Data.Aspiration teacher);
        Task UpdateAsync(Data.Aspiration teacher);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Aspiration> teachers);
    }
}
