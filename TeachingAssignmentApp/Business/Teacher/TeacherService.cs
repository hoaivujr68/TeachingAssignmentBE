using AutoMapper;
using TeachingAssignmentApp.Model;
using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc;

namespace TeachingAssignmentApp.Business.Teacher
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

        public async Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel, string? role = "lanhdao")
        {
            return await _teacherRepository.GetAllAsync(queryModel, role);
        }

        public async Task<TeacherModel> GetByIdAsync(Guid id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            return _mapper.Map<TeacherModel>(teacher);
        }

        public async Task<Data.Teacher> GetByNameAsync(string name)
        {
            return await _teacherRepository.GetByNameAsync(name);
        }

        public async Task AddAsync(TeacherModel teacherModel)
        {
            var newTeacher = _mapper.Map<Data.Teacher>(teacherModel);
            await _teacherRepository.AddAsync(newTeacher);
        }

        public async Task UpdateAsync(TeacherModel teacherModel)
        {
            var updateTeacher = _mapper.Map<Data.Teacher>(teacherModel);
            await _teacherRepository.UpdateAsync(updateTeacher);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _teacherRepository.DeleteAsync(id);
        }

        public async Task<bool> ImportTeachersAfterAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var queryModel = new QueryModel();
            queryModel.PageSize = 100;
            var teachers = await _teacherRepository.GetAllAsync(queryModel);
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    // Tạo Dictionary để lưu classCode và tổng gdTeaching
                    var classCodeDictionary = new Dictionary<string, double>();

                    for (int row = 299; row <= rowCount; row++)
                    {
                        string gdTeachingText = worksheet.Cells[row, 20].Text.Trim();
                        if (!double.TryParse(gdTeachingText, out var gdTeaching) || gdTeaching == 0)
                        {
                            // Bỏ qua dòng nếu gdTeaching không hợp lệ
                            continue;
                        }

                        var classCode = worksheet.Cells[row, 6].Text.Trim();

                        // Kiểm tra nếu classCode đã tồn tại trong Dictionary
                        if (classCodeDictionary.ContainsKey(classCode))
                        {
                            classCodeDictionary[classCode] += gdTeaching;
                        }
                        else
                        {
                            classCodeDictionary[classCode] = gdTeaching;
                        }
                    }

                    foreach (var entry in classCodeDictionary)
                    {
                        var classCode = entry.Key;       // classCode
                        var totalGdTeaching = entry.Value; // Tổng gdTeaching của classCode

                        // Tìm danh sách teachers có khóa học trong ListCourse trùng với classCode
                        var matchingTeachers = teachers.Content
                            .Where(teacher => teacher.ProfessionalGroup
                                .Any(pg => pg.ListCourse.Any(course => course.Name == classCode)))
                            .ToList();

                        if (matchingTeachers.Count > 0)
                        {
                            // Tính gdTeaching chia đều
                            double distributedGdTeaching = totalGdTeaching / matchingTeachers.Count;

                            // Cập nhật gdTeaching cho từng teacher
                            foreach (var teacher in matchingTeachers)
                            {
                                teacher.GdTeaching += distributedGdTeaching;
                            }
                        }
                    }
                }
            }

            var teacherEntities = teachers.Content.Select(tm => new Data.Teacher
            {
                Id = tm.Id, // Chỉ cần Id để xác định bản ghi
                GdTeaching = Math.Round(tm.GdTeaching ?? 0.0, 2)
            }).ToList();

            await _teacherRepository.UpdateRangeGdTeachingAsync(teacherEntities);

            return true;
        }
        public async Task<bool> ImportTeachersAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var teachers = new List<Data.Teacher>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    HashSet<string> processedTeachers = new();
                    // Khởi tạo biến đếm bên ngoài vòng lặp
                    var teacherCounter = 1;

                    for (int row = 8; row <= rowCount; row++)
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
                            var teacher = new Data.Teacher
                            {
                                Id = Guid.NewGuid(),
                                Code = $"GV{teacherCounter:D3}",
                                Name = teacherName,
                                GdTeaching = gdTeaching,
                            };

                            teachers.Add(teacher);
                            processedTeachers.Add(teacherName);
                            teacherCounter++;
                        }
                    }
                }
            }

            await _teacherRepository.AddRangeAsync(teachers);
            return true;
        }

        public FileContentResult DownloadTeacherTemplate()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Template", "TeacherTemplate.xlsx");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The template file does not exist.");
            }

            // Đọc file thành byte array
            var fileBytes = File.ReadAllBytes(filePath);

            // Trả file về dạng FileContentResult
            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "TeacherTemplate.xlsx"
            };
        }
    }
}
