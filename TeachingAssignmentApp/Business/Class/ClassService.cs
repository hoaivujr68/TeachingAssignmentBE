using AutoMapper;
using OfficeOpenXml;
using TeachingAssignmentApp.Business.Course;
using TeachingAssignmentApp.Business.ProfessionalGroup;
using TeachingAssignmentApp.Business.Project;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Class
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classeRepository;
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ICourseRepository _courseRepository;
        public ClassService(
            IClassRepository classeRepository,
            IMapper mapper,
            IProjectService projectService,
            ICourseRepository courseRepository
            )
        {
            _classeRepository = classeRepository;
            _mapper = mapper;
            _projectService = projectService;
            _courseRepository = courseRepository;
        }

        public async Task<Pagination<ClassModel>> GetAllAsync(QueryModel queryModel, string? role = "lanhdao")
        {
            return await _classeRepository.GetAllAsync(queryModel, role);
        }

        public async Task<ClassModel> GetByIdAsync(Guid id)
        {
            var classe = await _classeRepository.GetByIdAsync(id);
            return _mapper.Map<ClassModel>(classe);
        }

        public async Task<Data.Class> GetByNameAsync(string name)
        {
            return await _classeRepository.GetByNameAsync(name);
        }

        public async Task AddAsync(ClassModel classeModel)
        {
            var newClass = _mapper.Map<Data.Class>(classeModel);
            await _classeRepository.AddAsync(newClass);
        }

        public async Task UpdateAsync(ClassModel classeModel)
        {
            var updateClass = _mapper.Map<Data.Class>(classeModel);
            await _classeRepository.UpdateAsync(updateClass);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _classeRepository.DeleteAsync(id);
        }

        public async Task<bool> ImportClassAsync(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("Please upload a valid Excel file.");
            }

            var classes = new List<Data.Class>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (ExcelPackage package = new(stream))
                {
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        throw new InvalidOperationException("No worksheets found in the uploaded file.");
                    }

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    HashSet<string> processedCourses = new();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        string classeId = worksheet.Cells[row, 3].Text.Trim();

                        if (string.IsNullOrEmpty(classeId))
                        {
                            continue;
                        }

                        string courseName = worksheet.Cells[row, 4].Text.Trim();
                        if (string.IsNullOrEmpty(courseName))
                        {
                            continue;
                        }

                        var course = await _courseRepository.GetByNameAsync(courseName);
                        if (course == null)
                        {
                            continue;
                        }

                        string timeTable = worksheet.Cells[row, 13].Text.Trim();
                        if (!processedCourses.Contains(classeId))
                        {
                            var classe = new Data.Class
                            {
                                Id = Guid.NewGuid(),
                                Name = worksheet.Cells[row, 5].Text.Trim(),
                                Code = worksheet.Cells[row, 3].Text.Trim(),
                                Type = worksheet.Cells[row, 12].Text.Trim(),
                                CourseName = worksheet.Cells[row, 4].Text.Trim(),
                                GroupName = worksheet.Cells[row, 8].Text.Trim(),
                                MaxEnrol = string.IsNullOrEmpty(worksheet.Cells[row, 11].Text.Trim()) ? 0 : int.Parse(worksheet.Cells[row, 11].Text.Trim()),
                                TimeTable = timeTable,
                                GdTeaching = string.IsNullOrEmpty(worksheet.Cells[row, 37].Text.Trim()) ? 0.0 : double.Parse(worksheet.Cells[row, 37].Text.Trim()),
                                TimeTableDetail = new List<TimeTableModel>()
                            };


                            if (timeTable != "HV liên hệ với giáo viên" && timeTable != null)
                            {
                                var schedules = timeTable.Split(new[] { "Sáng", "Chiều" }, StringSplitOptions.RemoveEmptyEntries);

                                foreach (var schedule in schedules)
                                {
                                    string seasion = timeTable.Contains("Sáng" + schedule) ? "Sáng" : "Chiều";
                                    var parts = schedule.Split(',');

                                    string day = parts[0].Split('T')[1].Trim().Replace(":", "");
                                    string classPeriod = parts[0].Split(' ')[3].Trim();
                                    string room = parts[1].Split(':')[1].Trim();
                                    string week = parts[2].Split(':')[1].Trim();
                                    for (int i = 3; i < parts.Length; i++)
                                    {
                                        week += ", " + parts[i].Trim();
                                    }

                                    var periods = classPeriod.Split('-');

                                    // Lấy giá trị bắt đầu và kết thúc
                                    int batDau = int.Parse(periods[0]);
                                    int ketThuc = int.Parse(periods[1]);

                                    // Điều chỉnh nếu là "Chiều"
                                    if (seasion == "Chiều")
                                    {
                                        batDau += 6;
                                        ketThuc += 6;
                                    }

                                    var period = Enumerable.Range(batDau, ketThuc - batDau + 1).ToArray();

                                    var timeTableModel = new TimeTableModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Day = day,
                                        Seasion = seasion,
                                        ClassPeriod = classPeriod,
                                        Room = room,
                                        Week = week,
                                        Period = period
                                    };

                                    classe.TimeTableDetail.Add(timeTableModel);
                                }
                            }
                            classes.Add(classe);
                            processedCourses.Add(classeId);
                        }
                    }
                }
            }

            await _classeRepository.AddRangeAsync(classes);
            return true;
        }
    }
}
