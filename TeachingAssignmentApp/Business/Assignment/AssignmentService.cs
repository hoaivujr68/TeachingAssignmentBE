using System.Linq;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Course;
using TeachingAssignmentApp.Business.ProfessionalGroup;
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
        public AssignmentService(
            ITeacherRepository teacherRepository,
            ICourseRepository courseRepository,
            IProfessionalGroupRepository professionalGroupRepository,
            IClassRepository classRepository,
            ITeachingAssignmentRepository teachingAssignmentRepository
            )
        {
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _professionalGroupRepository = professionalGroupRepository;
            _classRepository = classRepository;
            _teachingAssignmentRepository = teachingAssignmentRepository;
        }

        public async Task<List<TeacherInputModel>> GetAllTeacherInfo()
        {
            var queryTeacherModel = new TeacherQueryModel();
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
            var queryClassModel = new ClassQueryModel();
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
    }
}
