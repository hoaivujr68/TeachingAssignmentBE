﻿using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Class
{
    public interface IClassService
    {
        Task<Pagination<ClassModel>> GetAllAsync(QueryModel queryModel, string? role = "lanhdao");
        Task<ClassModel> GetByIdAsync(Guid id);
        Task<Data.Class> GetByNameAsync(string name);
        Task AddAsync(ClassModel classe);
        Task UpdateAsync(ClassModel classe);
        Task DeleteAsync(Guid id);
        Task<bool> ImportClassAsync(IFormFile file);
        FileContentResult DownloadTeacherTemplate();
    }
}
