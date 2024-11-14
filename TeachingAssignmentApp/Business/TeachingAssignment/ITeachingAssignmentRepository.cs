﻿using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.TeachingAssignment
{
    public interface ITeachingAssignmentRepository
    {
        Task<Pagination<Data.TeachingAssignment>> GetAllAsync(TeachingAssignmentQueryModel queryModel);
        Task AddAsync(Data.TeachingAssignment teachingAssignment);
        Task AddRangeAsync(IEnumerable<Data.TeachingAssignment> teachingAssignments);
        Task UpdateAsync(Data.TeachingAssignment teachingAssignment);
        Task DeleteAsync(Guid id);
        Task<Pagination<ClassModel>> GetClassNotAssignmentAsync(ClassQueryModel queryModel);
    }
}
