using AutoMapper;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;
using TeachingAssignmentApp.Repositories;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.InkML;

namespace TeachingAssignmentApp.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper _mapper;

        public TeacherService(ITeacherRepository teacherRepository, IMapper mapper)
        {
            _teacherRepository = teacherRepository;
            _mapper = mapper;
        }

        public async Task<Pagination<TeacherModel>> GetAllAsync(TeacherQueryModel queryModel)
        {
            return await _teacherRepository.GetAllAsync(queryModel);
        }

        public async Task<TeacherModel> GetByIdAsync(Guid id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            return _mapper.Map<TeacherModel>(teacher);
        }

        public async Task<Teacher> GetByNameAsync(string name)
        {
            return await _teacherRepository.GetByNameAsync(name);
        }

        public async Task AddAsync(TeacherModel teacherModel)
        {
            var newTeacher = _mapper.Map<Teacher>(teacherModel);
            await _teacherRepository.AddAsync(newTeacher);
        }

        public async Task UpdateAsync(TeacherModel teacherModel)
        {
            var updateTeacher = _mapper.Map<Teacher>(teacherModel);
            await _teacherRepository.UpdateAsync(updateTeacher);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _teacherRepository.DeleteAsync(id);
        }

        public async Task<bool> ImportTeachersAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var teachers = new List<Teacher>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    HashSet<string> processedTeachers = new();
                    for (int row = 2; row <= rowCount; row++)
                    {
                        string teacherName = worksheet.Cells[row, 2].Text.Trim();
                        string gdTeachingText = worksheet.Cells[row, 21].Text.Trim();
                        // Kiểm tra nếu tên giảng viên rỗng hoặc không có giá trị GdTeaching hợp lệ
                        if (string.IsNullOrEmpty(teacherName) ||
                            !double.TryParse(gdTeachingText, out var gdTeaching) ||
                            gdTeaching == 0)
                        {
                            // Bỏ qua dòng này nếu không đáp ứng điều kiện
                            continue;
                        }
                        if (!processedTeachers.Contains(teacherName))
                        {
                            var teacher = new Teacher
                            {
                                Id = Guid.NewGuid(),
                                Name = teacherName,
                                GdTeaching = gdTeaching,
                            };

                            teachers.Add(teacher);
                            processedTeachers.Add(teacherName);
                        }
                    }
                }
            }

            await _teacherRepository.AddRangeAsync(teachers);
            return true;
        }
    }
}