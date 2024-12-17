using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Project;
using TeachingAssignmentApp.Business.Teacher;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Aspiration
{
    public class AspirationService : IAspirationService
    {
        private readonly IAspirationRepository _aspirationRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly TeachingAssignmentDbContext _context;
        private readonly IClassRepository _classRepository;
        private readonly IProjectRepository _projectRepository;
        public AspirationService(
            IAspirationRepository aspirationRepository, 
            IMapper mapper, 
            ITeacherRepository teacherRepository,
            IProjectService projectService,
            IClassRepository classRepository,
            TeachingAssignmentDbContext context,
            IProjectRepository projectRepository
            )
        {
            _aspirationRepository = aspirationRepository;
            _mapper = mapper;
            _teacherRepository = teacherRepository;
            _projectService = projectService;
            _classRepository = classRepository;
            _context = context;
            _projectRepository = projectRepository;
        }

        public async Task<Pagination<AspirationModel>> GetAllAsync(QueryModel queryModel)
        {
            return await _aspirationRepository.GetAllAsync(queryModel);
        }

        public async Task<AspirationModel> GetByIdAsync(Guid id)
        {
            var aspiration = await _aspirationRepository.GetByIdAsync(id);
            return _mapper.Map<AspirationModel>(aspiration);
        }

        public async Task AddAsync(AspirationModel aspirationModel)
        {
            var newAspiration = _mapper.Map<Data.Aspiration>(aspirationModel);
            await _aspirationRepository.AddAsync(newAspiration);
        }

        public async Task UpdateAsync(AspirationModel aspirationModel)
        {
            var updateAspiration = _mapper.Map<Data.Aspiration>(aspirationModel);
            await _aspirationRepository.UpdateAsync(updateAspiration);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _aspirationRepository.DeleteAsync(id);
        }

        public async Task<bool> ImportAspirationsAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var aspirations = new List<Data.Aspiration>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 3; row <= rowCount; row++)
                    {
                        string studentName = worksheet.Cells[row, 3].Text.Trim();

                        if (string.IsNullOrEmpty(studentName))
                        {
                            // Bỏ qua dòng này nếu không đáp ứng điều kiện
                            continue;
                        }

                        string desireAccept = worksheet.Cells[row, 16].Text.Trim();
                        if (string.IsNullOrEmpty(desireAccept))
                        {
                            continue;
                        }
                        var teacher1 = await _teacherRepository.GetByNameAsync(worksheet.Cells[row, 17].Text.Trim());
                        if (teacher1 == null)
                        {
                            continue;
                        }
                        var teacher2 = await _teacherRepository.GetByNameAsync(worksheet.Cells[row, 18].Text.Trim());
                        if (teacher2 == null)
                        {
                            continue;
                        }
                        var teacher3 = await _teacherRepository.GetByNameAsync(worksheet.Cells[row, 19].Text.Trim());
                        if (teacher3 == null)
                        {
                            continue;
                        }

                        var studentId = worksheet.Cells[row, 2].Text.Trim();
                        var project = await _projectRepository.GetByStudentIdAsync(studentId);
                        if (project == null)
                        {
                            continue;
                        }

                        var teacherName = desireAccept == "Chờ xác nhận" ? null : worksheet.Cells[row, 17].Text.Trim();
                        var teacherCode = string.IsNullOrEmpty(teacherName) ? null : "";

                        if (!string.IsNullOrEmpty(teacherName))
                        {
                            var teacher = await _teacherRepository.GetByNameAsync(teacherName);
                            if (teacher == null)
                            {
                                continue;
                            }
                            teacherName = teacher?.Name;
                            teacherCode = teacher?.Code;
                        }

                        var matchingItem = ProjectHourEnum.Items
                            .FirstOrDefault(item => item.Name == worksheet.Cells[row, 8].Text.Trim() && item.Type == worksheet.Cells[row, 6].Text.Trim());
                        if (matchingItem == null)
                        {
                            continue;
                        }

                        var aspiration = new Data.Aspiration
                        {
                            Id = Guid.NewGuid(),
                            TeacherCode = teacherCode,
                            TeacherName = teacherName,
                            StudentId = worksheet.Cells[row, 2].Text.Trim(),
                            StudentName = studentName,
                            Topic = worksheet.Cells[row, 7].Text.Trim(),
                            ClassName = worksheet.Cells[row, 8].Text.Trim(),
                            GroupName = worksheet.Cells[row, 6].Text.Trim(),
                            Status = worksheet.Cells[row, 15].Text.Trim(),
                            DesireAccept = worksheet.Cells[row, 16].Text.Trim(),
                            Aspiration1 = worksheet.Cells[row, 17].Text.Trim(),
                            Aspiration2 = worksheet.Cells[row, 18].Text.Trim(),
                            Aspiration3 = worksheet.Cells[row, 19].Text.Trim(),
                            StatusCode = desireAccept == "Chờ xác nhận" ? 0 : 1,
                            GdInstruct = matchingItem.Value
                        };

                        aspirations.Add(aspiration);

                    }
                }
            }

            await _aspirationRepository.AddRangeAsync(aspirations); 
            var totalGdTeaching = await _classRepository.GetTotalGdTeachingAsync();
            var totalGdInstruct = aspirations.Sum(a => a.GdInstruct);
            var test = await _projectService.GetTotalGdInstruct();
            totalGdInstruct += test;
            var proportion = Math.Round((totalGdInstruct ?? 0) / totalGdTeaching, 2);

            // Lấy danh sách giáo viên từ DbContext
            var teachers = await _context.Teachers.ToListAsync();

            foreach (var teacher in teachers)
            {
                if (teacher.GdTeaching.HasValue)
                {
                    teacher.GdInstruct = Math.Round(teacher.GdTeaching.Value * proportion, 2);
                }
            }

            // Cập nhật danh sách giáo viên
            _context.Teachers.UpdateRange(teachers);
            await _context.SaveChangesAsync();
            return true;
        }

        public FileContentResult DownloadTeacherTemplate()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Template", "AspirationTemplate.xlsx");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The template file does not exist.");
            }

            // Đọc file thành byte array
            var fileBytes = File.ReadAllBytes(filePath);

            // Trả file về dạng FileContentResult
            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "AspirationTemplate.xlsx"
            };
        }
    }
}
