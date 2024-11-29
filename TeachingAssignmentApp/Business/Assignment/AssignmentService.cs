using Microsoft.EntityFrameworkCore;
using System.Linq;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Course;
using TeachingAssignmentApp.Business.ProfessionalGroup;
using TeachingAssignmentApp.Business.Project;
using TeachingAssignmentApp.Business.ProjectAssigment;
using TeachingAssignmentApp.Business.Teacher;
using TeachingAssignmentApp.Business.TeacherProfessionalGroup;
using TeachingAssignmentApp.Business.TeachingAssignment;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Assignment
{
    public class AssignmentService : IAssignmentService
    {
        private readonly int maxIterations = 1000;
        private readonly double par = 0.3;
        private readonly int hms = 100;
        private readonly double hmcr = 0.7;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IProfessionalGroupRepository _professionalGroupRepository;
        private readonly IClassRepository _classRepository;
        private readonly ITeachingAssignmentRepository _teachingAssignmentRepository;
        private readonly IAspirationRepository _aspirationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
        private readonly ITeacherProfessionalGroupRepository _teacherProfessionalGroupRepository;
        private readonly TeachingAssignmentDbContext _context;
        public AssignmentService(
            ITeacherRepository teacherRepository,
            ICourseRepository courseRepository,
            IProfessionalGroupRepository professionalGroupRepository,
            IClassRepository classRepository,
            ITeachingAssignmentRepository teachingAssignmentRepository,
            IAspirationRepository aspirationRepository,
            IProjectRepository projectRepository,
            IProjectAssignmentRepository projectAssignmentRepository,
            ITeacherProfessionalGroupRepository teacherProfessionalGroupRepository,
            TeachingAssignmentDbContext context
            )
        {
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _professionalGroupRepository = professionalGroupRepository;
            _classRepository = classRepository;
            _teachingAssignmentRepository = teachingAssignmentRepository;
            _aspirationRepository = aspirationRepository;
            _projectRepository = projectRepository;
            _projectAssignmentRepository = projectAssignmentRepository;
            _teacherProfessionalGroupRepository = teacherProfessionalGroupRepository;
            _context = context;
        }

        public async Task<List<TeacherInputModel>> GetAllTeacherInfo()
        {
            var queryTeacherModel = new QueryModel();
            queryTeacherModel.PageSize = 200;
            queryTeacherModel.CurrentPage = 1;
            var teachers = await _teacherRepository.GetAllAsync(queryTeacherModel);
            if (teachers.Content == null)
            {
                throw new Exception("Teacher not found");
            }

            var teacherInfoList = new List<TeacherInputModel>();

            foreach (var teacher in teachers.Content)
            {
                var teacherProfessionalGroup = await _teacherProfessionalGroupRepository.GetByTeacherIdAsync(teacher.Id);
                if (teacherProfessionalGroup.Count() == 0)
                {
                    continue;
                }
                var courses = await _courseRepository.GetByTeacherIdAsync(teacher.Id);
                var courseList = new List<CourseInputModel>();

                foreach (var course in courses)
                {
                    var classCodes = await _classRepository.GetByCourseNameAsync(course.Name);
                    var codeList = classCodes.Select(c => c).ToList();
                    if (codeList.Any())
                    {
                        courseList.Add(new CourseInputModel
                        {
                            Code = codeList,
                            Name = course.Name
                        });
                    }

                }

                var teacherInfo = new TeacherInputModel
                {
                    Code = teacher.Code,
                    GdTeaching = teacher.GdTeaching,
                    GdInstruct = teacher.GdInstruct,
                    ListCourse = courseList
                };

                teacherInfoList.Add(teacherInfo);
            }


            return teacherInfoList;
        }

        public async Task<List<ClassInputModel>> GetAllClassInfo()
        {
            var queryClassModel = new QueryModel();
            queryClassModel.PageSize = 300;
            queryClassModel.CurrentPage = 1;
            var classes = await _classRepository.GetAllAsync(queryClassModel);
            if (classes.Content == null)
            {
                throw new Exception("Class not found");
            }

            var classInfoList = new List<ClassInputModel>();
            foreach (var classe in classes.Content)
            {
                var classInfo = new ClassInputModel
                {
                    Name = classe.Name,
                    Code = classe.Code,
                    Type = classe.Type,
                    CourseName = classe.CourseName,
                    GroupName = classe.GroupName,
                    MaxEnrol = classe.MaxEnrol,
                    GdTeaching = classe.GdTeaching,
                    TimeTableDetail = classe.TimeTableDetail
                };

                classInfoList.Add(classInfo);
            }

            return classInfoList;
        }

        public static List<List<(ClassInputModel Class, TeacherInputModel? Teacher)>> InitHarmonyMemoryTeaching(List<TeacherInputModel> teachers, List<ClassInputModel> classes, int memorySize)
        {
            var harmonyMemory = new List<List<(ClassInputModel Class, TeacherInputModel? Teacher)>>();
            var random = new Random();

            for (int i = 0; i < memorySize; i++)
            {
                var solution = new List<(ClassInputModel, TeacherInputModel?)>();

                var teacherWorkloads = teachers.ToDictionary(t => t.Code, t => 0.0);

                foreach (var classEntry in classes)
                {
                    string classKey = classEntry.Code;

                    // Lọc ra danh sách giảng viên hợp lệ
                    var validTeachers = teachers
                        .Where(t =>
                            t.ListCourse != null &&
                            t.ListCourse.Any(course => course.Name == classEntry.CourseName) &&
                            teacherWorkloads[t.Code] + classEntry.GdTeaching <= 2 * (t.GdTeaching ?? 0)
                        )
                        .ToList();

                    TeacherInputModel? assignedTeacher = null;

                    if (validTeachers.Any())
                    {
                        assignedTeacher = validTeachers[random.Next(validTeachers.Count)];
                        teacherWorkloads[assignedTeacher.Code] += classEntry.GdTeaching;
                    }

                    solution.Add((classEntry, assignedTeacher));
                    // Thêm vào giải pháp
                }

                harmonyMemory.Add(solution);
            }
            return harmonyMemory;
        }

        private static bool SchedulesOverlap(List<int> period1, List<int> period2)
        {
            return period1.Intersect(period2).Any();
        }

        private static int EvaluateSolutionTeaching(
    List<(ClassInputModel Class, TeacherInputModel? Teacher)> solution,
    List<TeacherInputModel> teachers,
    List<ClassInputModel> classes)
        {
            int totalScore = 0;

            var teacherWorkloads = teachers.ToDictionary(t => t.Code, t => 0.0);
            var teacherSchedules = teachers.ToDictionary(t => t.Code, t => new List<(string Day, List<int> Periods)>());
            var assignedTeachers = new HashSet<string>(); // Danh sách các giảng viên đã được phân công

            foreach (var (classItem, teacher) in solution)
            {
                if (teacher != null) // Kiểm tra nếu giảng viên không null
                {
                    var classInfo = classItem;
                    var teacherData = teacher;

                    // Tăng khối lượng công việc cho giảng viên
                    teacherWorkloads[teacher.Code] += classInfo.GdTeaching;
                    assignedTeachers.Add(teacher.Code); // Đánh dấu giảng viên đã được phân công

                    // Kiểm tra trùng lịch
                    foreach (var timeTable in classInfo.TimeTableDetail)
                    {
                        var day = timeTable.Day;
                        var period = timeTable.Period.ToList();

                        // Kiểm tra nếu lịch trùng
                        foreach (var (scheduledDay, scheduledPeriods) in teacherSchedules[teacher.Code])
                        {
                            if (day == scheduledDay && SchedulesOverlap(scheduledPeriods, period))
                            {
                                totalScore -= 10; // Phạt nếu lịch trùng
                                break;
                            }
                        }

                        // Thêm lịch mới vào lịch trình của giảng viên
                        teacherSchedules[teacher.Code].Add((day, period));
                    }

                    // Phạt nếu vượt giờ giảng dạy cho phép
                    if (teacherWorkloads[teacher.Code] > 2 * (teacherData.GdTeaching ?? 0))
                    {
                        totalScore -= 10;
                    }
                    else
                    {
                        totalScore += 1; // Thưởng nếu phân công hợp lệ
                    }
                }
            }

            // Phạt nếu có giảng viên không được phân công lớp
            var unassignedTeachers = teachers.Where(t => !assignedTeachers.Contains(t.Code)).ToList();
            if (unassignedTeachers.Any())
            {
                totalScore -= unassignedTeachers.Count * 5;
            }

            return totalScore;
        }

        private static List<List<(ClassInputModel Class, TeacherInputModel? Teacher)>> UpdateHarmonyTeaching(
            List<List<(ClassInputModel Class, TeacherInputModel? Teacher)>> harmonyMemory,
            List<TeacherInputModel> teachers,
            List<ClassInputModel> classes,
            List<(ClassInputModel Class, TeacherInputModel? Teacher)> newSolution)
        {
            var worstSolution = harmonyMemory.OrderBy(sol => EvaluateSolutionTeaching(sol, teachers, classes)).First();
            var worstScore = EvaluateSolutionTeaching(worstSolution, teachers, classes);
            var newScore = EvaluateSolutionTeaching(newSolution, teachers, classes);

            if (newScore > worstScore)
            {
                harmonyMemory.Remove(worstSolution);
                harmonyMemory.Add(newSolution);
            }
            return harmonyMemory;
        }

        public static (Dictionary<ClassInputModel, TeacherInputModel>, int) RunHarmonySearchTeaching(
    List<TeacherInputModel> teachers,
    List<ClassInputModel> classes,
    int memorySize = 100,
    int maxIterations = 1000)
        {
            // Sắp xếp giáo viên theo `time_gl` tăng dần
            var sortedTeachers = teachers.OrderBy(t => t.GdTeaching).ToList();

            // Khởi tạo bộ nhớ hài hòa
            var harmonyMemory = InitHarmonyMemoryTeaching(teachers, classes, memorySize);

            // Tiến hành chạy thuật toán Harmony Search
            for (int i = 0; i < maxIterations; i++)
            {
                // Tạo một giải pháp mới
                var newSolution = new Dictionary<ClassInputModel, TeacherInputModel>();
                var teacherWorkload = teachers.ToDictionary(t => t, t => 0.0);

                // Phân công giảng viên cho các lớp học
                foreach (var classItem in classes)
                {
                    // Chọn ngẫu nhiên một giải pháp từ bộ nhớ hòa âm
                    var randomSolution = harmonyMemory.ElementAt(new Random().Next(harmonyMemory.Count));
                    var selectedTeacher = randomSolution.FirstOrDefault(s => s.Class.Code == classItem.Code).Teacher;

                    // Kiểm tra xem giáo viên đã chọn có hợp lệ không
                    if (selectedTeacher != null &&
                        classItem.CourseName != null &&
                        selectedTeacher.ListCourse.Any(c => c.Name == classItem.CourseName) &&
                        teacherWorkload[selectedTeacher] + classItem.GdTeaching <= 2 * selectedTeacher.GdTeaching)
                    {
                        newSolution[classItem] = selectedTeacher;
                        teacherWorkload[selectedTeacher] += classItem.GdTeaching;
                    }
                    else
                    {
                        // Ưu tiên phân công giáo viên có `time_gl` thấp trong danh sách hợp lệ
                        var validTeachers = sortedTeachers.Where(t =>
                            t.ListCourse != null &&
                            t.ListCourse.Any(c => c.Name == classItem.CourseName) &&
                            teacherWorkload[t] + classItem.GdTeaching <= 2 * t.GdTeaching).ToList();

                        if (validTeachers.Any())
                        {
                            var assignedTeacher = validTeachers.First();
                            newSolution[classItem] = assignedTeacher;
                            teacherWorkload[assignedTeacher] += classItem.GdTeaching;
                        }
                        else
                        {
                            newSolution[classItem] = null;  // Không thể gán nếu không hợp lệ
                        }
                    }
                }

                // Cập nhật bộ nhớ hài hòa (giải pháp hiện tại vào bộ nhớ hài hòa)
                var newSolutionList = newSolution
                    .Select(kvp => (Class: kvp.Key, Teacher: kvp.Value))  // Convert to List of Tuples
                    .ToList();

                harmonyMemory = UpdateHarmonyTeaching(harmonyMemory, teachers, classes, newSolutionList);
            }

            // Tìm giải pháp tốt nhất từ bộ nhớ hài hòa
            var bestSolution = harmonyMemory.OrderByDescending(sol => EvaluateSolutionTeaching(sol, teachers, classes)).First();
            var bestScore = EvaluateSolutionTeaching(bestSolution, teachers, classes);

            // Convert the best solution (List of Tuples) to a Dictionary
            var bestSolutionDict = bestSolution.ToDictionary(
                assignment => assignment.Class,
                assignment => assignment.Teacher
            );
            var unassignedClasses = bestSolutionDict.Where(kvp => kvp.Value == null)
                                                    .Select(kvp => kvp.Key)
                                                    .ToList();

            // Lấy danh sách giảng viên chưa được phân công
            var assignedTeachers = bestSolutionDict.Values.Where(t => t != null).Distinct().ToList();
            var unassignedTeachers = teachers.Where(t => !assignedTeachers.Contains(t)).ToList();

            return (bestSolutionDict, bestScore);
        }

        public async Task<SolutionModel> TeachingAssignment()
        {
            // Xóa dữ liệu phân công hiện có
            var teachingAssignment = _context.TeachingAssignments.ToListAsync();
            _context.TeachingAssignments.RemoveRange(await teachingAssignment);

            // Lấy thông tin giảng viên và lớp học
            var teacherInfoList = await GetAllTeacherInfo();
            var classInfoList = await GetAllClassInfo();

            Dictionary<ClassInputModel, TeacherInputModel> bestSolutionDict;
            int bestScore;

            // Tìm giải pháp ban đầu bằng Harmony Search
            (bestSolutionDict, bestScore) = RunHarmonySearchTeaching(teacherInfoList, classInfoList);

            // Lấy danh sách lớp chưa được phân công
            var unassignedClasses = bestSolutionDict.Where(kvp => kvp.Value == null)
                                                    .Select(kvp => kvp.Key)
                                                    .ToList();

            // Lấy danh sách giảng viên chưa được phân công
            var assignedTeachers = bestSolutionDict.Values.Where(t => t != null).Distinct().ToList();
            var unassignedTeachers = teacherInfoList.Where(t => !assignedTeachers.Contains(t)).ToList();
            var teacherTotalHours = assignedTeachers.ToDictionary(teacher => teacher.Code, teacher =>
            {
                // Tính tổng số giờ đã được phân công cho giảng viên này từ bestSolutionDict
                return bestSolutionDict
                       .Where(kvp => kvp.Value == teacher) // Lọc các lớp đã phân công cho giảng viên
                       .Sum(kvp => kvp.Key.GdTeaching);   // Tổng số giờ GdTeaching của các lớp
            });

            while (unassignedClasses.Any())
            {
                var newlyAssignedClasses = new List<ClassInputModel>();

                foreach (var unassignedClass in unassignedClasses)
                {
                    // Tìm giảng viên phù hợp
                    var validTeachers = assignedTeachers
                        .Where(teacher =>
                            //teacher.ListCourse.Any(course => course.Name == unassignedClass.CourseName) &&
                            (teacherTotalHours[teacher.Code] + unassignedClass.GdTeaching) <= 2 * (teacher.GdTeaching ?? 0))
                        .ToList();

                    if (validTeachers.Any())
                    {
                        // Chọn ngẫu nhiên giảng viên từ danh sách hợp lệ
                        var selectedTeacher = validTeachers.OrderBy(_ => Guid.NewGuid()).First();
                        bestSolutionDict[unassignedClass] = selectedTeacher;
                        teacherTotalHours[selectedTeacher.Code] += unassignedClass.GdTeaching;

                        newlyAssignedClasses.Add(unassignedClass);
                    }
                }

                // Loại bỏ các lớp đã phân công trong vòng lặp này
                unassignedClasses = unassignedClasses.Except(newlyAssignedClasses).ToList();
            }

            // Tạo SolutionModel từ giải pháp cập nhật
            var bestSolutionModel = new SolutionModel
            {
                ClassAssignments = bestSolutionDict.ToDictionary(
                    kvp => kvp.Key.Code,
                    kvp => kvp.Value?.Code // Giá trị có thể là null nếu vẫn không phân được
                )
            };

            await SaveTeachingAssignments(Task.FromResult(bestSolutionModel));
            return bestSolutionModel;
        }

        public async Task SaveTeachingAssignments(Task<SolutionModel> bestSolutionTask)
        {
            // Chờ để lấy kết quả từ Task<SolutionModel>
            var bestSolution = await bestSolutionTask;

            var teachingAssignments = new List<Data.TeachingAssignment>();

            // Tạo một danh sách mới để chứa các courseAssignment có teacherCode là null
            var coursesWithNoTeacher = new List<KeyValuePair<string, string>>();
            foreach (var courseAssignment in bestSolution.ClassAssignments)
            {
                var courseCode = courseAssignment.Key;
                var teacherCode = courseAssignment.Value;

                if (teacherCode == null)
                {
                    coursesWithNoTeacher.Add(courseAssignment);
                    continue; // Tiếp tục vòng lặp mà không xử lý phần còn lại cho courseAssignment này
                }
                var teacher = await _teacherRepository.GetByCodeAsync(teacherCode);
                if (teacher == null)
                {
                    throw new Exception($"Teacher with code {teacherCode} not found");
                }
                var course = await _classRepository.GetByCodeAsync(courseCode);
                if (course == null)
                {
                    throw new Exception($"Course with code {courseCode} not found");
                }
                var teachingAssignment = new Data.TeachingAssignment
                {
                    Id = Guid.NewGuid(),
                    Name = course.Name,
                    TeacherCode = teacherCode,
                    Code = courseCode,
                    GdTeaching = course.GdTeaching,
                    Type = course.Type,
                    GroupName = course.GroupName,
                    TimeTable = course.TimeTable,
                    CourseName = course.CourseName,
                    MaxEnrol = course.MaxEnrol,
                    TimeTableDetail = course.TimeTableDetail
                };

                teachingAssignments.Add(teachingAssignment);
            }
            await _teachingAssignmentRepository.AddRangeAsync(teachingAssignments);
        }

        public async Task<List<AspirationInputModel>> GetAllAspirationInfo()
        {
            var queryProjectModel = new QueryModel();
            queryProjectModel.PageSize = 1500;
            queryProjectModel.CurrentPage = 1;

            var aspirations = await _aspirationRepository.GetAllAsync(queryProjectModel);

            var aspirationInfoList = new List<AspirationInputModel>();

            foreach (var aspiration in aspirations.Content)
            {
                var project = await _projectRepository.GetByCourseNameAsync(aspiration.ClassName);
                if (project == null)
                {
                    continue;
                }

                var teacher1 = await _teacherRepository.GetByNameAsync(aspiration.Aspiration1);
                if (teacher1 == null)
                {
                    continue;
                }
                var teacher2 = await _teacherRepository.GetByNameAsync(aspiration.Aspiration2);
                if (teacher2 == null)
                {
                    continue;
                }
                var teacher3 = await _teacherRepository.GetByNameAsync(aspiration.Aspiration3);
                if (teacher3 == null)
                {
                    continue;
                }

                var aspirationInfo = new AspirationInputModel
                {
                    TeacherCode = aspiration.TeacherCode,
                    StudentId = aspiration.StudentId,
                    StudentName = aspiration.StudentName,
                    Topic = aspiration.Topic,
                    ClassName = aspiration.ClassName,
                    GroupName = aspiration.GroupName,
                    Status = aspiration.Status,
                    DesireAccept = aspiration.DesireAccept,
                    Aspiration1 = aspiration.Aspiration1,
                    Aspiration2 = aspiration.Aspiration2,
                    Aspiration3 = aspiration.Aspiration3,
                    StatusCode = aspiration.StatusCode,
                    GdInstruct = project.GdInstruct,
                    Aspiration1Code = teacher1?.Code,
                    Aspiration2Code = teacher2?.Code,
                    Aspiration3Code = teacher3?.Code,
                };

                aspirationInfoList.Add(aspirationInfo);
            }
            // Lọc ra chỉ các phần tử đầu tiên có StudentId duy nhất
            aspirationInfoList = aspirationInfoList
                .GroupBy(a => a.StudentId)      // Nhóm theo StudentId
                .Select(g => g.First())          // Lấy phần tử đầu tiên trong mỗi nhóm
                .ToList();

            return aspirationInfoList;
        }

        public static List<List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)>> InitializeHarmonyMemory(
            List<TeacherInputModel> teachers, List<AspirationInputModel> aspirations, int memorySize)
        {
            var harmonyMemory = new List<List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)>>();
            var random = new Random();
            var lockedAssignments = new Dictionary<string, string>();

            // Gán trực tiếp các aspirations có teacher
            foreach (var aspiration in aspirations)
            {
                if (!string.IsNullOrEmpty(aspiration.TeacherCode))
                {
                    lockedAssignments[aspiration.StudentId] = aspiration.TeacherCode;
                }
            }

            for (int i = 0; i < memorySize; i++)
            {
                var solution = new List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)>();
                foreach (var aspiration in aspirations)
                {
                    if (lockedAssignments.TryGetValue(aspiration.StudentId, out var lockedTeacherCode))
                    {
                        // Tìm giáo viên đã khóa
                        var lockedTeacher = teachers.FirstOrDefault(t => t.Code == lockedTeacherCode);
                        solution.Add((aspiration, lockedTeacher));
                    }
                    else
                    {
                        // Phân công ngẫu nhiên từ danh sách giáo viên khả dụng
                        var validTeachers = teachers.ToList();
                        var assignedTeacher = validTeachers.Any()
                            ? validTeachers[random.Next(validTeachers.Count)]
                            : null;
                        solution.Add((aspiration, assignedTeacher));
                    }
                }
                harmonyMemory.Add(solution);
            }

            return harmonyMemory;
        }

        public static int EvaluateSolution(
            List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)> solution,
            List<TeacherInputModel> teachers)
        {
            int totalScore = 0;
            var teacherWorkload = teachers.ToDictionary(t => t.Code, t => 0.0);

            foreach (var (Aspiration, Teacher) in solution)
            {
                if (Teacher != null)
                {
                    teacherWorkload[Teacher.Code] += Aspiration.GdInstruct ?? 0.0;

                    // Phạt nếu vượt quá giờ
                    if (teacherWorkload[Teacher.Code] > 1.5 * Teacher.GdInstruct)
                    {
                        totalScore -= 10; // Điểm phạt nếu vượt giờ
                    }
                    else
                    {
                        totalScore += 1; // Điểm thưởng nếu phân công hợp lệ
                    }
                }
            }

            return totalScore;
        }

        public static void UpdateHarmony(
            List<List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)>> harmonyMemory,
            List<TeacherInputModel> teachers,
            List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)> newSolution)
        {
            var worstSolution = harmonyMemory
                .OrderBy(sol => EvaluateSolution(sol, teachers))
                .First();

            var worstScore = EvaluateSolution(worstSolution, teachers);
            var newScore = EvaluateSolution(newSolution, teachers);

            if (newScore > worstScore)
            {
                harmonyMemory.Remove(worstSolution);
                harmonyMemory.Add(newSolution);
            }
        }

        public static (Dictionary<AspirationInputModel, TeacherInputModel>, int)
            HarmonySearchAlgorithm(
                List<TeacherInputModel> teachers,
                List<AspirationInputModel> aspirations,
                int memorySize = 100,
                int maxIterations = 1000,
                double hmcr = 0.9,
                double par = 0.3)
        {
            // Khởi tạo bộ nhớ hài hòa
            var harmonyMemory = InitializeHarmonyMemory(teachers, aspirations, memorySize);

            // Theo dõi tổng giờ đã phân công và số nguyện vọng đã phân công
            var teacherWorkload = teachers.ToDictionary(t => t.Code, t => 0.0);
            var teacherAssignments = teachers.ToDictionary(t => t.Code, t => 0);

            var random = new Random();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                var newSolution = new List<(AspirationInputModel Aspiration, TeacherInputModel? Teacher)>();

                foreach (var aspiration in aspirations)
                {
                    TeacherInputModel? assignedTeacher = null;

                    if (!string.IsNullOrEmpty(aspiration.TeacherCode))
                    {
                        // Gán cứng phân công nếu đã có giáo viên
                        assignedTeacher = teachers.FirstOrDefault(t => t.Code == aspiration.TeacherCode);

                        // Kiểm tra điều kiện về giờ và số lượng nguyện vọng
                        if (assignedTeacher != null &&
                            (teacherWorkload[assignedTeacher.Code] + aspiration.GdInstruct > 1.5 * assignedTeacher.GdInstruct ||
                             teacherAssignments[assignedTeacher.Code] > 30))
                        {
                            assignedTeacher = null; // Không thể gán nếu vượt ngưỡng
                        }
                    }
                    else
                    {
                        // Chọn ngẫu nhiên từ bộ nhớ hài hòa
                        var randomSolution = harmonyMemory[random.Next(harmonyMemory.Count)];
                        assignedTeacher = randomSolution
                            .FirstOrDefault(sol => sol.Aspiration.StudentId == aspiration.StudentId)
                            .Teacher;

                        // Kiểm tra điều kiện về giờ và số lượng nguyện vọng
                        if (assignedTeacher != null &&
                            (teacherWorkload[assignedTeacher.Code] + aspiration.GdInstruct > 1.5 * assignedTeacher.GdInstruct ||
                             teacherAssignments[assignedTeacher.Code] >= 30))
                        {
                            assignedTeacher = null; // Không thể gán nếu vượt ngưỡng
                        }
                    }

                    if (assignedTeacher != null)
                    {
                        teacherWorkload[assignedTeacher.Code] += aspiration.GdInstruct ?? 0.0;
                        teacherAssignments[assignedTeacher.Code]++;
                    }

                    newSolution.Add((aspiration, assignedTeacher));
                }

                // Cập nhật bộ nhớ hài hòa
                UpdateHarmony(harmonyMemory, teachers, newSolution);
            }

            // Tìm giải pháp tốt nhất
            var bestSolution = harmonyMemory
                .OrderByDescending(sol => EvaluateSolution(sol, teachers))
                .First();

            var bestSolutionDict = bestSolution.ToDictionary(
                    assignment => assignment.Aspiration,
                    assignment => assignment.Teacher
                );

            var bestScore = EvaluateSolution(bestSolution, teachers);
            // In ra giảng viên chưa được phân công và nguyện vọng chưa được phân công từ bestSolution
            var unassignedTeachers = teachers.Where(t => !bestSolution.Any(sol => sol.Teacher?.Code == t.Code)).ToList();
            if (unassignedTeachers.Any())
            {
                Console.WriteLine("Unassigned Teachers: " + string.Join(", ", unassignedTeachers.Select(t => t.Code)));
            }

            var unassignedAspirations = aspirations.Where(a => !bestSolution.Any(sol => sol.Aspiration.StudentId == a.StudentId && sol.Teacher != null)).ToList();
            if (unassignedAspirations.Any())
            {
                Console.WriteLine("Unassigned Aspirations: " + string.Join(", ", unassignedAspirations.Select(a => a.StudentId.ToString())));
            }

            return (bestSolutionDict, bestScore);
        }

        public async Task<SolutionProjectModel> ProjectAssignment()
        {
            var projectAssigments = await _context.ProjectAssigments.ToListAsync();
            _context.ProjectAssigments.RemoveRange(projectAssigments);
            await _context.SaveChangesAsync(); // Save changes after removing assignments

            var aspirationInfoList = await GetAllAspirationInfo();
            var teacherInfoList = await GetAllTeacherInfo();

            var (bestSolutionDict, bestScore) = HarmonySearchAlgorithm(teacherInfoList, aspirationInfoList);


            var unassignedAspirations = bestSolutionDict.Where(kvp => kvp.Value == null)
                                                    .Select(kvp => kvp.Key)
                                                    .ToList();
            var assignedTeachers = bestSolutionDict.Values.Where(t => t != null).Distinct().ToList();
            var unassignedTeachers = teacherInfoList.Where(t => !assignedTeachers.Contains(t)).ToList();
            var teacherTotalHours = assignedTeachers.ToDictionary(teacher => teacher.Code, teacher =>
            {
                // Tính tổng số giờ đã được phân công cho giảng viên này từ bestSolutionDict
                return bestSolutionDict
                       .Where(kvp => kvp.Value == teacher) // Lọc các lớp đã phân công cho giảng viên
                       .Sum(kvp => kvp.Key.GdInstruct);   // Tổng số giờ GdTeaching của các lớp
            });
            var teacherTotalAssignments = teacherInfoList.ToDictionary(teacher => teacher.Code, teacher =>
            {
                return bestSolutionDict
                    .Count(kvp => kvp.Value?.Code == teacher.Code);  // Đếm số nguyện vọng đã phân công cho giảng viên
            });
            while (unassignedAspirations.Any())
            {
                var newlyAssignedAspirations = new List<AspirationInputModel>();
                foreach (var aspiration in unassignedAspirations)
                {

                    var availableTeacher = teacherInfoList.FirstOrDefault(t =>
                        !bestSolutionDict.ContainsKey(aspiration) ||  // Nếu nguyện vọng chưa được phân công
                        teacherTotalAssignments[t.Code] < 30 &&  // Kiểm tra xem giảng viên có phân công quá 30 nguyện vọng không
                        (teacherTotalHours[t.Code] + aspiration.GdInstruct) <= 1.5 * (t.GdInstruct ?? 0)  // Kiểm tra số giờ giảng dạy
                    );

                    if (availableTeacher != null)
                    {
                        bestSolutionDict[aspiration] = availableTeacher;
                        // Cập nhật số giờ giảng dạy và số nguyện vọng đã phân công
                        teacherTotalHours[availableTeacher.Code] += aspiration.GdInstruct;
                        teacherTotalAssignments[availableTeacher.Code] += 1;// Gán giảng viên cho nguyện vọng
                        newlyAssignedAspirations.Add(aspiration);
                    }
                }

                unassignedAspirations = unassignedAspirations.Except(newlyAssignedAspirations).ToList();
            }

            // Tạo SolutionModel từ giải pháp cập nhật
            var bestSolutionModel = new SolutionProjectModel
            {
                AspirationAssignments = bestSolutionDict
                    .GroupBy(kvp => kvp.Key.StudentId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.FirstOrDefault().Value?.Code
                    )
            };

            await SaveProjectAssignments(Task.FromResult(bestSolutionModel));
            return bestSolutionModel;
        }


        public async Task SaveProjectAssignments(Task<SolutionProjectModel> bestSolutionTask)
        {
            // Chờ để lấy kết quả từ Task<SolutionModel>
            var bestSolution = await bestSolutionTask;

            var projectAssigments = new List<Data.ProjectAssigment>();

            // Tạo một danh sách mới để chứa các courseAssignment có teacherCode là null
            var aspirationWithNoTeacher = new List<KeyValuePair<string, string>>();
            foreach (var projectAssignment in bestSolution.AspirationAssignments)
            {
                var studenId = projectAssignment.Key;
                var teacherCode = projectAssignment.Value;

                if (teacherCode == null)
                {
                    aspirationWithNoTeacher.Add(projectAssignment);
                    continue; // Tiếp tục vòng lặp mà không xử lý phần còn lại cho projectAssignment này
                }

                var teacher = await _teacherRepository.GetByCodeAsync(teacherCode);
                if (teacher == null)
                {
                    continue;
                }

                var aspiration = await _aspirationRepository.GetByStudentIdAsync(studenId);
                if (aspiration == null)
                {
                    throw new Exception($"Course with code {studenId} not found");
                }
                var project = await _projectRepository.GetByCourseNameAsync(aspiration.ClassName);
                if (project == null)
                {
                    continue;
                }

                var assignment = new Data.ProjectAssigment
                {
                    Id = Guid.NewGuid(),
                    TeacherCode = teacherCode,
                    StudentId = studenId,
                    StudentName = aspiration.StudentName,
                    Topic = aspiration.Topic,
                    ClassName = aspiration.ClassName,
                    GroupName = aspiration.GroupName,
                    Status = aspiration.Status,
                    DesireAccept = aspiration.DesireAccept,
                    Aspiration1 = aspiration.Aspiration1,
                    Aspiration2 = aspiration.Aspiration2,
                    Aspiration3 = aspiration.Aspiration3,
                    GdInstruct = project.GdInstruct,
                    StatusCode = aspiration.StatusCode,
                    TeacherName = teacher.Name
                };

                projectAssigments.Add(assignment);
            }
            await _projectAssignmentRepository.AddRangeAsync(projectAssigments);
        }

    }
}
