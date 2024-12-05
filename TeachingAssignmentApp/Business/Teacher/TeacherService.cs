using AutoMapper;
using TeachingAssignmentApp.Model;
using OfficeOpenXml;

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

        public async Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel, string? role = "Leader")
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
    }
}