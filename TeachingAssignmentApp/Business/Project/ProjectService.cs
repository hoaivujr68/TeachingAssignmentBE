﻿using AutoMapper;
using OfficeOpenXml;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public ProjectService(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<Pagination<ProjectModel>> GetAllAsync(ProjectQueryModel queryModel)
        {
            return await _projectRepository.GetAllAsync(queryModel);
        }

        public async Task<ProjectModel> GetByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            return _mapper.Map<ProjectModel>(project);
        }

        public async Task<Data.Project> GetByNameAsync(string name)
        {
            return await _projectRepository.GetByNameAsync(name);
        }

        public async Task AddAsync(ProjectModel projectModel)
        {
            var newProject = _mapper.Map<Data.Project>(projectModel);
            await _projectRepository.AddAsync(newProject);
        }

        public async Task UpdateAsync(ProjectModel projectModel)
        {
            var updateProject = _mapper.Map<Data.Project>(projectModel);
            await _projectRepository.UpdateAsync(updateProject);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _projectRepository.DeleteAsync(id);
        }

        public async Task<double> GetTotalGdInstruct()
        {
            var query = new ProjectQueryModel();
            query.CurrentPage = 1;
            query.PageSize = 1500;
            var projects = await _projectRepository.GetAllAsync(query);
            return projects.Content.Sum(p => p.GdInstruct) ?? 0.0;
        }

        public async Task<bool> ImportProjectsAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var projects = new List<Data.Project>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        string courseName = worksheet.Cells[row, 2].Text.Trim();
                        string name = worksheet.Cells[row, 3].Text.Trim();
                        string groupName = worksheet.Cells[row, 11].Text.Trim();

                        if (string.IsNullOrEmpty(courseName) || string.IsNullOrEmpty(name))
                        {
                            // Bỏ qua dòng này nếu không đáp ứng điều kiện
                            continue;
                        }

                        // Tìm kiếm phần tử trong ProjectHourEnum.Items dựa trên điều kiện
                        var matchingItem = ProjectHourEnum.Items
                            .FirstOrDefault(item => item.Name == courseName && item.Type == groupName);
                        if (matchingItem == null)
                        {
                            continue;
                        }
                        var project = new Data.Project
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            Code = worksheet.Cells[row, 1].Text.Trim(),
                            CourseName = courseName,
                            Type = worksheet.Cells[row, 4].Text.Trim(),
                            StudenId = worksheet.Cells[row, 6].Text.Trim(),
                            StudentName = worksheet.Cells[row, 7].Text.Trim(),
                            GroupName = groupName,
                            GdInstruct = matchingItem.Value
                        };

                        projects.Add(project);
                    }
                }
            }

            await _projectRepository.AddRangeAsync(projects);
            return true;
        }
    }
}