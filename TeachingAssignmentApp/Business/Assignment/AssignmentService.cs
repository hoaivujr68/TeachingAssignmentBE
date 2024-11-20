using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Course;
using TeachingAssignmentApp.Business.ProfessionalGroup;
using TeachingAssignmentApp.Business.Project;
using TeachingAssignmentApp.Business.ProjectAssigment;
using TeachingAssignmentApp.Business.Teacher;
using TeachingAssignmentApp.Business.TeachingAssignment;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Assignment
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IProfessionalGroupRepository _professionalGroupRepository;
        private readonly IClassRepository _classRepository;
        private readonly ITeachingAssignmentRepository _teachingAssignmentRepository;
        private readonly IAspirationRepository _aspirationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
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

        public static SolutionModel RandomSolution(List<TeacherInputModel> teachers, List<ClassInputModel> classes)
        {
            var solution = new SolutionModel();
            var remainingClasses = classes.Select(c => c.Code).ToList();
            var teachersAssigned = new HashSet<string>();
            foreach (var teacher in teachers)
            {
                var possibleClasses = remainingClasses
                    .Where(cls => teacher.ListCourse.Any(course => course.Code.Contains(cls)))
                    .ToList();

                if (possibleClasses.Any())
                {
                    var assignedClass = possibleClasses[new Random().Next(possibleClasses.Count)];
                    solution.ClassAssignments[assignedClass] = teacher.Code;
                    teachersAssigned.Add(teacher.Code);
                    remainingClasses.Remove(assignedClass);
                }
            }

            foreach (var cls in remainingClasses)
            {
                var possibleTeachers = teachers
                    .Where(teacher => teacher.ListCourse.Any(course => course.Code.Contains(cls)))
                    .ToList();

                if (possibleTeachers.Any())
                {
                    var teacherChosen = possibleTeachers[new Random().Next(possibleTeachers.Count)];
                    solution.ClassAssignments[cls] = teacherChosen.Code;
                    teachersAssigned.Add(teacherChosen.Code);
                }
                else
                {
                    solution.ClassAssignments[cls] = null;  // No teacher can teach this class
                }
            }
            return solution;
        }

        public int ObjectiveFunction(SolutionModel solution, List<ClassInputModel> classes)
        {
            int noTeacherClasses = 0;

            foreach (var cls in classes)
            {
                if (!solution.ClassAssignments.ContainsKey(cls.Code) || solution.ClassAssignments[cls.Code] == null)
                {
                    noTeacherClasses++;
                }
            }

            return noTeacherClasses;
        }


        public async Task<SolutionModel> TeachingAssignment()
        {
            var teachingAssignment = _context.TeachingAssignments.ToListAsync();
            _context.TeachingAssignments.RemoveRange(await teachingAssignment);

            var teacherInfoList = await GetAllTeacherInfo();
            var classInfoList = await GetAllClassInfo();
            var solutions = new List<SolutionModel>();
            var listFiness = new List<int>();

            for (int i = 0; i < 25; i++)
            {
                var solution = RandomSolution(teacherInfoList, classInfoList);
                solutions.Add(solution);
                listFiness.Add(ObjectiveFunction(solution, classInfoList));
            }
            var bestSolution = solutions[listFiness.IndexOf(listFiness.Min())];
            await SaveTeachingAssignments(Task.FromResult(bestSolution));
            return bestSolution;
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

            return aspirationInfoList;
        }

        public async Task<SolutionProjectModel> ProjectAssignment()
        {
            var projectAssigments = _context.ProjectAssigments.ToListAsync();
            _context.ProjectAssigments.RemoveRange(await projectAssigments);

            var aspirationInfoList = await GetAllAspirationInfo();
            var solutions = new List<SolutionProjectModel>();
            var listFiness = new List<int>();
            var queryTeacherModel = new QueryModel();
            queryTeacherModel.PageSize = 200;
            queryTeacherModel.CurrentPage = 1;
            var teachers = await _teacherRepository.GetAllAsync(queryTeacherModel);
            if (teachers.Content == null)
            {
                throw new Exception("Teacher not found");
            }

            for (int i = 0; i < 25; i++)
            {
                var solution = RandomProjectSolution(teachers.Content, aspirationInfoList);
                solutions.Add(solution);
                listFiness.Add(ObjectiveProjectFunction(solution, aspirationInfoList));
            }

            var bestSolution = solutions[listFiness.IndexOf(listFiness.Max())];
            await SaveProjectAssignments(Task.FromResult(bestSolution));
            return bestSolution;
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
                var project = await _projectRepository.GetByStudentIdAsync(studenId);
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

        public int ObjectiveProjectFunction(SolutionProjectModel solution, List<AspirationInputModel> aspirationInfoList)
        {
            int totalScore = 0;

            foreach (var assignment in solution.AspirationAssignments)
            {
                var studenId = assignment.Key;
                var teacherCode = assignment.Value;

                var aspiration = aspirationInfoList.FirstOrDefault(a => a.StudentId == studenId);

                if (aspiration != null)
                {
                    // Kiểm tra nguyện vọng 1, 2, và 3 và tính điểm
                    if (teacherCode == aspiration.Aspiration1Code)
                    {
                        totalScore += 4; // Nguyện vọng 1
                    }
                    else if (teacherCode == aspiration.Aspiration2Code)
                    {
                        totalScore += 3; // Nguyện vọng 2
                    }
                    else if (teacherCode == aspiration.Aspiration3Code)
                    {
                        totalScore += 2; // Nguyện vọng 3
                    }
                }
            }

            return totalScore;
        }


        public static SolutionProjectModel RandomProjectSolution(IEnumerable<TeacherModel> teachers, List<AspirationInputModel> aspirations)
        {
            var solutionProject = new SolutionProjectModel();
            var teacherLoad = teachers.ToDictionary(t => t.Code, t => 0.0);
            var aspirationCount = teachers.ToDictionary(t => t.Code, t => new Dictionary<string, int>());
            var aspirationTotal = teachers.ToDictionary(t => t.Code, t => 0);

            foreach (var asp in aspirations)
            {
                bool assigned = false;

                foreach (var priority in new[] { "Aspiration1Code", "Aspiration2Code", "Aspiration3Code" })
                {
                    string preferredTeacher = null;
                    switch (priority)
                    {
                        case "Aspiration1Code":
                            preferredTeacher = asp.Aspiration1Code;
                            break;
                        case "Aspiration2Code":
                            preferredTeacher = asp.Aspiration2Code;
                            break;
                        case "Aspiration3Code":
                            preferredTeacher = asp.Aspiration3Code;
                            break;
                    }

                    var teacher = teachers.FirstOrDefault(t => t.Code == preferredTeacher);
                    if (teacherLoad[preferredTeacher] + (asp.GdInstruct ?? 0.0) <= teacher.GdInstruct
                        && aspirationTotal.GetValueOrDefault(preferredTeacher, 0) < 30
                        && aspirationCount.GetValueOrDefault(preferredTeacher, new Dictionary<string, int>())
                                           .GetValueOrDefault(asp.ClassName, 0) < 5)
                    {
                        solutionProject.AspirationAssignments[asp.StudentId] = preferredTeacher;
                        teacherLoad[preferredTeacher] += asp.GdInstruct ?? 0.0;
                        aspirationTotal[preferredTeacher]++;

                        if (!aspirationCount[preferredTeacher].ContainsKey(asp.ClassName))
                            aspirationCount[preferredTeacher][asp.ClassName] = 0;
                        aspirationCount[preferredTeacher][asp.ClassName]++;

                        assigned = true;
                        break;
                    }
                }

                // Nếu không thể gán theo nguyện vọng, chọn ngẫu nhiên giảng viên phù hợp với ràng buộc
                if (!assigned)
                {
                    var availableTeachers = teachers
                        .Where(t => teacherLoad[t.Code] + asp.GdInstruct <= t.GdInstruct &&
                                    aspirationTotal[t.Code] < 30 &&
                                     aspirationCount[t.Code][asp.ClassName] < 5)
                        .ToList();

                    if (availableTeachers.Any())
                    {
                        var teacherChosen = availableTeachers[new Random().Next(availableTeachers.Count)];
                        solutionProject.AspirationAssignments[asp.StudentId] = teacherChosen.Code;
                        teacherLoad[teacherChosen.Code] += asp.GdInstruct ?? 0.0;
                        aspirationTotal[teacherChosen.Code]++;

                        if (!aspirationCount[teacherChosen.Code].ContainsKey(asp.ClassName))
                            aspirationCount[teacherChosen.Code][asp.ClassName] = 0;
                        aspirationCount[teacherChosen.Code][asp.ClassName]++;
                    }
                    else
                    {
                        var teachersList = teachers.ToList();
                        var teacherChosen = teachersList[new Random().Next(teachersList.Count)];
                        solutionProject.AspirationAssignments[asp.StudentId] = teacherChosen.Code;
                    }
                }
            }

            // Đảm bảo tất cả giảng viên có ít nhất một lớp gán
            var unassignedTeachers = teachers
                .Where(t => !solutionProject.AspirationAssignments.ContainsValue(t.Code))
                .ToList();

            foreach (var teacher in unassignedTeachers)
            {
                var unassignedClasses = aspirations
                    .Where(c => !solutionProject.AspirationAssignments.ContainsKey(c.StudentId))
                    .ToList();

                if (unassignedClasses.Any())
                {
                    var assignedClass = unassignedClasses[new Random().Next(unassignedClasses.Count)];
                    solutionProject.AspirationAssignments[assignedClass.StudentId] = teacher.Code;
                }
                else
                {
                    // Nếu tất cả lớp đã được gán, chọn một lớp ngẫu nhiên để gán cho giảng viên
                    var randomClass = aspirations[new Random().Next(aspirations.Count)];
                    solutionProject.AspirationAssignments[randomClass.StudentId] = teacher.Code;
                }
            }

            return solutionProject;
        }

    }
}
