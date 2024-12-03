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
using TeachingAssignmentApp.Migrations;
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
                                totalScore -= 20; // Phạt nếu lịch trùng
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
            int maxIterations = 1000,
            double hmcr = 0.9, // Harmony Memory Consideration Rate
            double par = 0.1 // Pitch Adjust Rate
        )
        {
            // Sắp xếp giáo viên theo `GdTeaching` (tương đương với `time_gl` trong Python)
            var sortedTeachers = teachers.OrderBy(t => t.GdTeaching).ToList();

            // Khởi tạo bộ nhớ hòa âm
            var harmonyMemory = InitHarmonyMemoryTeaching(teachers, classes, memorySize);

            // Tiến hành chạy thuật toán Harmony Search
            for (int i = 0; i < maxIterations; i++)
            {
                var newSolution = new List<(ClassInputModel Class, TeacherInputModel? Teacher)>();
                   
                var teacherWorkload = teachers.ToDictionary(t => t.Code, t => 0.0);

                var teacherSchedules = teachers.ToDictionary(
                    t => t.Code,
                    t => t.Schedule.SelectMany(
                        schedule =>
                            new[] { (schedule.Day, schedule.Periods.ToList()) } // Bao bọc tuple trong mảng
                    ).ToList()
                );


                foreach (var classItem in classes)
                {
                    // Chọn ngẫu nhiên một giải pháp từ bộ nhớ hòa âm
                    var randomSolution = harmonyMemory[new Random().Next(harmonyMemory.Count)];
                    var selectedTeacher = randomSolution.FirstOrDefault(s => s.Class.Code == classItem.Code).Teacher;

                    if (selectedTeacher != null &&
                        IsValidAssignment(selectedTeacher, classItem, teachers, teacherWorkload, teacherSchedules))
                    {
                        // Phân công giáo viên hợp lệ
                        AssignTeacherToClass(selectedTeacher, classItem, newSolution, teacherWorkload, teacherSchedules);
                    }
                    else
                    {
                        // Tìm giáo viên hợp lệ từ danh sách sắp xếp
                        var validTeachers = FindValidTeachers(classItem, sortedTeachers, teachers, teacherWorkload, teacherSchedules);

                        if (validTeachers.Any())
                        {
                            var assignedTeacher = validTeachers.First();
                            AssignTeacherToClass(assignedTeacher, classItem, newSolution, teacherWorkload, teacherSchedules);
                        }
                        else
                        {
                            newSolution.Add((classItem, null));
                        }
                    }
                }

                // Cập nhật bộ nhớ hòa âm (giải pháp hiện tại vào bộ nhớ hòa âm)
                var newSolutionList = newSolution;
                   //.Select(kvp => (Class: kvp.Key, Teacher: kvp.Value))  // Convert to List of Tuples
                   //.ToList();
                harmonyMemory = UpdateHarmonyTeaching(harmonyMemory, teachers, classes, newSolutionList);
            }

            // Tìm giải pháp tốt nhất từ bộ nhớ hòa âm
            var bestSolution = harmonyMemory.OrderByDescending(sol => EvaluateSolutionTeaching(sol, teachers, classes)).First();
            var bestScore = EvaluateSolutionTeaching(bestSolution, teachers, classes);

            var teacherScheduleBest = RebuildTeacherSchedules(bestSolution, teachers, classes);

            // Convert the best solution (List of Tuples) to a Dictionary
            var bestSolutionDict = bestSolution.ToDictionary(
                assignment => assignment.Class,
                assignment => assignment.Teacher
            );

            // Tìm các lớp chưa được phân công
            var unassignedClasses = bestSolutionDict.Where(kvp => kvp.Value == null)
                                                    .Select(kvp => kvp.Key)
                                                    .ToList();
            bestSolution = ReassignUnassignedClasses(unassignedClasses, bestSolution, teacherScheduleBest, teachers);

            bestSolutionDict = bestSolution.ToDictionary(
                assignment => assignment.Class,
                assignment => assignment.Teacher
            );

            unassignedClasses = bestSolutionDict.Where(kvp => kvp.Value == null)
                                                    .Select(kvp => kvp.Key)
                                                    .ToList();
            // Lấy danh sách giảng viên chưa được phân công
            var assignedTeachers = bestSolutionDict.Values.Where(t => t != null).Distinct().ToList();
            var unassignedTeachers = teachers.Where(t => !assignedTeachers.Contains(t)).ToList();

            return (bestSolutionDict, bestScore);
        }

        public static Dictionary<string, List<(string Day, List<int> Periods)>> RebuildTeacherSchedules(
            List<(ClassInputModel Class, TeacherInputModel? Teacher)> solution,
            List<TeacherInputModel> teachers,
            List<ClassInputModel> classes)
        {
            var teacherSchedules = teachers.ToDictionary(t => t.Code, t => new List<(string Day, List<int> Periods)>());

            foreach (var (classItem, teacher) in solution)
            {
                if (teacher == null) continue;
                var classInfo = classItem;
                var teacherData = teacher;
                foreach (var timeTable in classInfo.TimeTableDetail)
                {
                    var day = timeTable.Day;
                    var period = timeTable.Period.ToList();
                    teacherSchedules[teacher.Code].Add((day, period));
                }
            }

            return teacherSchedules;
        }

        public static bool IsValidAssignment(
            TeacherInputModel teacher,
            ClassInputModel classInfo,
            List<TeacherInputModel> teachers,
            Dictionary<string, double> teacherWorkload,
            Dictionary<string, List<(string Day, List<int> Periods)>> teacherSchedules)
        {
            if (teacher == null || classInfo == null) return false;

            // Kiểm tra môn học có nằm trong danh sách các môn mà giáo viên có thể dạy không
            if (teacher.ListCourse == null ||
                !teacher.ListCourse.Any(course => course.Name == classInfo.CourseName))
            {
                return false;
            }

            // Kiểm tra nếu tổng giờ giảng dạy vượt quá giới hạn 1.7 lần giờ giảng dạy của giáo viên
            if (teacher.GdTeaching == null ||
                teacherWorkload[teacher.Code] + classInfo.GdTeaching > 1.7 * teacher.GdTeaching)
            {
                return false;
            }

            // Kiểm tra trùng lặp lịch dạy
            if (teacherSchedules.ContainsKey(teacher.Code))
            {
                foreach (var timetable in classInfo.TimeTableDetail)
                {
                    foreach (var (day, periods) in teacherSchedules[teacher.Code])
                    {
                        if (day == timetable.Day && SchedulesOverlap(periods, timetable.Period.ToList()))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static List<TeacherInputModel> FindValidTeachers(
            ClassInputModel classInfo,
            List<TeacherInputModel> sortedTeachers,
            List<TeacherInputModel> teachers,
            Dictionary<string, double> teacherWorkload,
            Dictionary<string, List<(string Day, List<int> Periods)>> teacherSchedules)
        {
            var validTeachers = new List<TeacherInputModel>();

            foreach (var teacher in sortedTeachers)
            {
                // Kiểm tra các điều kiện hợp lệ
                if (teacher.ListCourse != null &&
                    teacher.ListCourse.Any(course => course.Name == classInfo.CourseName) && // Kiểm tra môn học
                    teacherWorkload.ContainsKey(teacher.Code) &&
                    teacherWorkload[teacher.Code] + classInfo.GdTeaching <= 1.7 * teacher.GdTeaching && // Kiểm tra giờ dạy
                    !HasScheduleConflict(teacher, classInfo, teacherSchedules)) // Kiểm tra xung đột lịch dạy
                {
                    validTeachers.Add(teacher);
                }
            }

            return validTeachers;
        }


        public static bool HasScheduleConflict(
            TeacherInputModel teacher,
            ClassInputModel classInfo,
            Dictionary<string, List<(string Day, List<int> Periods)>> teacherSchedules)
        {
            if (teacher == null || classInfo == null || classInfo.TimeTableDetail == null)
                return false;

            // Duyệt qua từng mục trong TimeTableDetail của lớp học
            foreach (var timetable in classInfo.TimeTableDetail)
            {
                if (teacherSchedules.TryGetValue(teacher.Code, out var schedules))
                {
                    // Kiểm tra nếu trùng ngày và có trùng tiết
                    if (schedules.Any(s => s.Day == timetable.Day && SchedulesOverlap(s.Periods, timetable.Period.ToList())))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void AssignTeacherToClass(
            TeacherInputModel teacher,
            ClassInputModel classInfo,
            List<(ClassInputModel Class, TeacherInputModel? Teacher)> solution,
            Dictionary<string, double> teacherWorkload,
            Dictionary<string, List<(string Day, List<int> Periods)>> teacherSchedules)
        {
            // Thêm giáo viên vào danh sách giải pháp
            solution.Add((classInfo, teacher));

            // Cập nhật khối lượng công việc của giáo viên
            if (!teacherWorkload.ContainsKey(teacher.Code))
            {
                teacherWorkload[teacher.Code] = 0;
            }
            teacherWorkload[teacher.Code] += classInfo.GdTeaching;

            // Cập nhật lịch dạy của giáo viên
            if (!teacherSchedules.ContainsKey(teacher.Code))
            {
                teacherSchedules[teacher.Code] = new List<(string Day, List<int> Periods)>();
            }

            foreach (var timetable in classInfo.TimeTableDetail)
            {
                teacherSchedules[teacher.Code].Add((timetable.Day, timetable.Period.ToList()));
            }
        }

        public static List<(ClassInputModel Class, TeacherInputModel? Teacher)> ReassignUnassignedClasses(
            List<ClassInputModel> unassignedClasses,
            List<(ClassInputModel Class, TeacherInputModel? Teacher)> solution,
            Dictionary<string, List<(string Day, List<int> Periods)>> teacherSchedules,
            List<TeacherInputModel> teachers)
        {
            // Tiếp tục phân công cho đến khi không còn lớp chưa được phân công
            while (unassignedClasses.Count > 0)
            {
                bool classAssigned = false;

                // Lưu trữ các lớp đã phân công trong vòng lặp này
                var assignedClasses = new List<ClassInputModel>();

                foreach (var unassignedClass in unassignedClasses.ToList())
                {
                    // Tìm giáo viên phù hợp
                    var validTeachers = teachers
                        .Where(teacher =>
                            !HasScheduleConflict(teacher, unassignedClass, teacherSchedules) &&
                            (
                                (solution.All(s => s.Teacher?.Code != teacher.Code) && unassignedClass.GdTeaching <= 1.7 * (teacher.GdTeaching ?? 0)) ||
                                (solution.Where(s => s.Teacher?.Code == teacher.Code)
                                         .Sum(s => s.Class.GdTeaching) + unassignedClass.GdTeaching <= 1.7 * (teacher.GdTeaching ?? 0))
                            )
                        )
                        .ToList();

                    if (validTeachers.Any())
                    {
                        // Chọn giáo viên có tỷ lệ công việc thấp nhất
                        var selectedTeacher = validTeachers
                            .OrderBy(teacher =>
                                solution.Where(s => s.Teacher?.Code == teacher.Code)
                                        .Sum(s => s.Class.GdTeaching) / (teacher.GdTeaching ?? 1))
                            .First();

                        // Kiểm tra lịch của giáo viên trước khi phân công
                        if (!HasScheduleConflict(selectedTeacher, unassignedClass, teacherSchedules))
                        {
                            var classEntry = solution.FirstOrDefault(entry => entry.Class == unassignedClass);

                            if (classEntry != default)
                            {
                                // Cập nhật giáo viên cho lớp
                                var updatedEntry = (classEntry.Class, selectedTeacher);
                                solution[solution.IndexOf(classEntry)] = updatedEntry;
                            }

                            // Cập nhật lịch cho giáo viên
                            if (!teacherSchedules.ContainsKey(selectedTeacher.Code))
                                teacherSchedules[selectedTeacher.Code] = new List<(string Day, List<int> Periods)>();

                            foreach (var tt in unassignedClass.TimeTableDetail)
                            {
                                // Chuyển đổi từ int[] sang List<int>
                                List<int> periodsList = tt.Period.ToList(); // Giả sử tt.Period là int[]
                                teacherSchedules[selectedTeacher.Code].Add((tt.Day, periodsList));
                            }

                            // Đánh dấu lớp đã được phân công
                            assignedClasses.Add(unassignedClass);
                            classAssigned = true; // Đã phân công lớp thành công
                        }
                    }
                }

                // Loại bỏ các lớp đã được phân công khỏi danh sách chưa được phân công
                foreach (var assignedClass in assignedClasses)
                {
                    unassignedClasses.Remove(assignedClass);
                }

                // Nếu không có lớp nào có thể phân công, thoát khỏi vòng lặp
                if (!classAssigned)
                    break;
            }

            return solution;
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
                    if (teacherWorkload[Teacher.Code] > 1.4 * Teacher.GdInstruct)
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
                            (teacherWorkload[assignedTeacher.Code] + aspiration.GdInstruct > 1.4 * assignedTeacher.GdInstruct ||
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
                            (teacherWorkload[assignedTeacher.Code] + aspiration.GdInstruct > 1.4 * assignedTeacher.GdInstruct ||
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
                    var availableTeacher = teacherInfoList
                       .Where(t =>
                           !bestSolutionDict.ContainsKey(aspiration) || // Nguyện vọng chưa được phân công
                           teacherTotalAssignments[t.Code] < 30 &&     // Số nguyện vọng phân công chưa vượt quá 30
                           (teacherTotalHours[t.Code] + aspiration.GdInstruct) <= 1.4 * (t.GdInstruct ?? 0)) // Tổng giờ giảng dạy không vượt quá giới hạn
                       .OrderBy(t => (teacherTotalHours[t.Code] / (t.GdInstruct ?? 1.0))) // Sắp xếp tăng dần theo tỷ lệ
                       .FirstOrDefault();
                    //var availableTeacher = teacherInfoList.FirstOrDefault(t =>
                    //    !bestSolutionDict.ContainsKey(aspiration) ||  // Nếu nguyện vọng chưa được phân công
                    //    teacherTotalAssignments[t.Code] < 30 &&  // Kiểm tra xem giảng viên có phân công quá 30 nguyện vọng không
                    //    (teacherTotalHours[t.Code] + aspiration.GdInstruct) <= 1.4 * (t.GdInstruct ?? 0)  // Kiểm tra số giờ giảng dạy
                    //);

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
