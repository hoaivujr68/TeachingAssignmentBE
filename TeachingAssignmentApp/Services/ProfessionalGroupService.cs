using AutoMapper;
using OfficeOpenXml;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;
using TeachingAssignmentApp.Repositories;

namespace TeachingAssignmentApp.Services
{
    public class ProfessionalGroupService : IProfessionalGroupService
    {
        private readonly IProfessionalGroupRepository _professionalGroupRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITeacherService _teacherService;
        private readonly ITeacherProfessionalGroupRepository _teacherProfessionalGroupRepository;
        private readonly IMapper _mapper;

        public ProfessionalGroupService(
            IProfessionalGroupRepository professionalGroupRepository,
            IMapper mapper,
            ITeacherService teacherService,
            ICourseRepository courseRepository,
            ITeacherProfessionalGroupRepository teacherProfessionalGroupRepository)
        {
            _professionalGroupRepository = professionalGroupRepository;
            _mapper = mapper;
            _teacherService = teacherService;
            _courseRepository = courseRepository;
            _teacherProfessionalGroupRepository = teacherProfessionalGroupRepository;
        }

        public async Task<Pagination<ProfessionalGroupModel>> GetAllAsync(ProfessionalGroupQueryModel queryModel)
        {
            return await _professionalGroupRepository.GetAllAsync(queryModel);
        }

        public async Task<ProfessionalGroupModel> GetByIdAsync(Guid id)
        {
            var professionalGroup = await _professionalGroupRepository.GetByIdAsync(id);
            return _mapper.Map<ProfessionalGroupModel>(professionalGroup);
        }

        public async Task AddAsync(ProfessionalGroupModel professionalGroupModel)
        {
            var newProfessionalGroup = _mapper.Map<ProfessionalGroup>(professionalGroupModel);
            await _professionalGroupRepository.AddAsync(newProfessionalGroup);
        }

        public async Task UpdateAsync(ProfessionalGroupModel professionalGroupModel)
        {
            var updateProfessionalGroup = _mapper.Map<ProfessionalGroup>(professionalGroupModel);
            await _professionalGroupRepository.UpdateAsync(updateProfessionalGroup);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _professionalGroupRepository.DeleteAsync(id);
        }

        public async Task<bool> ImportProfessionalGroupsAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var professionalGroups = new List<ProfessionalGroup>();

            var courses = new List<Course>();

            var teacherProfessionalGroups = new List<TeacherProfessionalGroup>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    HashSet<string> processedProfessionalGroups = new();
                    for (int row = 7; row <= rowCount; row++)
                    {
                        string professionalGroupName = worksheet.Cells[row, 2].Text.Trim();
                        string listCourseText = worksheet.Cells[row, 3].Text;
                        List<string> courseNames = listCourseText.Split(',').Select(c => c.Trim()).ToList();
                        string teacherName = worksheet.Cells[row, 4].Text.Trim();

                        // Bỏ qua dòng này nếu không đáp ứng điều kiện
                        if (string.IsNullOrEmpty(professionalGroupName) || string.IsNullOrEmpty(listCourseText) || string.IsNullOrEmpty(teacherName))
                        {
                            continue;
                        }

                        // Tìm hoặc tạo Teacher
                        var teacher = await _teacherService.GetByNameAsync(teacherName);
                        if (teacher == null)
                        {
                            continue;
                        }

                        // Tìm hoặc tạo ProfessionalGroup
                        ProfessionalGroup professionalGroup = null;
                        if (!processedProfessionalGroups.Contains(professionalGroupName))
                        {
                            professionalGroup = new ProfessionalGroup
                            {
                                Id = Guid.NewGuid(),
                                Name = professionalGroupName,
                            };

                            professionalGroups.Add(professionalGroup);
                            processedProfessionalGroups.Add(professionalGroupName);
                        }
                        else
                        {
                            professionalGroup = professionalGroups.First(pg => pg.Name == professionalGroupName);
                        }

                        // Tạo mối quan hệ nhiều-nhiều giữa Teacher và ProfessionalGroup
                        if (!teacherProfessionalGroups.Any(tpg => tpg.TeacherId == teacher.Id && tpg.ProfessionalGroupId == professionalGroup.Id))
                        {
                            var teacherProfessionalGroup = new TeacherProfessionalGroup
                            {
                                Id = Guid.NewGuid(),
                                TeacherId = teacher.Id,
                                Teacher = teacher,
                                ProfessionalGroupId = professionalGroup.Id,
                                ProfessionalGroup = professionalGroup
                            };
                            teacherProfessionalGroups.Add(teacherProfessionalGroup);
                        }

                        // Thêm các Course vào danh sách courses nếu chưa tồn tại
                        foreach (var courseName in courseNames)
                        {
                            if (!courses.Any(c => c.Name == courseName && c.ProfessionalGroupId == professionalGroup.Id && c.TeacherId == teacher.Id))
                            {
                                var course = new Course
                                {
                                    Id = Guid.NewGuid(),
                                    Name = courseName,
                                    ProfessionalGroupId = professionalGroup.Id,
                                    TeacherId = teacher.Id
                                };
                                courses.Add(course);
                            }
                        }
                    }
                }
            }
            try
            {
                await _professionalGroupRepository.AddRangeAsync(professionalGroups);
                await _teacherProfessionalGroupRepository.AddRangeAsync(teacherProfessionalGroups);
                await _courseRepository.AddRangeAsync(courses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
            return true;
        }
    }
}
