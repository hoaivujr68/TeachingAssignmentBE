using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Course;
using TeachingAssignmentApp.Business.CuckooAssignment;
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
    public class CuckooAssignmentService : ICuckooAssignmentService
    {
        private readonly int maxIterations = 1000;
        private readonly double par = 0.3;
        private readonly int hms = 100;
        private readonly double hmcr = 0.7;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IProfessionalGroupRepository _professionalGroupRepository;
        private readonly IClassRepository _classRepository;
        private readonly ICuckooTeachingRepository _cuckooTeachingRepository;
        private readonly IAspirationRepository _aspirationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
        private readonly ITeacherProfessionalGroupRepository _teacherProfessionalGroupRepository;
        private readonly TeachingAssignmentDbContext _context;

        public CuckooAssignmentService(
            ITeacherRepository teacherRepository,
            ICourseRepository courseRepository,
            IProfessionalGroupRepository professionalGroupRepository,
            IClassRepository classRepository,
            ICuckooTeachingRepository teachingAssignmentRepository,
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
            _cuckooTeachingRepository = teachingAssignmentRepository;
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

        public async Task<SolutionModel> TeachingAssignmentCuckooSearch()
        {
            // Lấy danh sách giảng viên và lớp học
            var teacherInfoList = await GetAllTeacherInfo();
            var classInfoList = await GetAllClassInfo();

            int popSize = 50; // Số lượng tổ chim cút
            int maxIter = 100; // Số lần lặp tối đa
            double pa = 0.25; // Tỷ lệ thay thế tổ xấu

            // Khởi tạo quần thể ban đầu
            var nests = InitializePopulation(classInfoList, teacherInfoList, popSize);

            // Lưu trữ tổ có fitness tốt nhất
            var bestNest = nests[0];
            int bestFitness = EvaluateFitness(bestNest, teacherInfoList);

            for (int iter = 0; iter < maxIter; iter++)
            {
                for (int i = 0; i < nests.Count; i++)
                {
                    var nest = nests[i];
                    var newNest = LevyFlight(nest, classInfoList, teacherInfoList);
                    int newFitness = EvaluateFitness(newNest, teacherInfoList);

                    // Nếu tổ mới tốt hơn, thay thế tổ cũ
                    if (newFitness > EvaluateFitness(nests[i], teacherInfoList))
                    {
                        nests[i] = newNest;
                    }

                    // Cập nhật tổ tốt nhất
                    if (newFitness > bestFitness)
                    {
                        bestNest = newNest;
                        bestFitness = newFitness;
                    }
                }

                // Loại bỏ tổ xấu với xác suất Pa
                ReplaceWorstNests(nests, pa, classInfoList, teacherInfoList);
            }

            // Chuyển tổ tốt nhất thành SolutionModel
            var bestSolutionModel = ConvertNestToSolutionModel(bestNest);
            await SaveTeachingAssignments(Task.FromResult(bestSolutionModel));
            return ConvertNestToSolutionModel(bestNest);
        }

        private List<Dictionary<ClassInputModel, TeacherInputModel>> InitializePopulation(
            List<ClassInputModel> classInfoList,
            List<TeacherInputModel> teacherInfoList,
            int popSize)
        {
            var nests = new List<Dictionary<ClassInputModel, TeacherInputModel>>();
            var random = new Random();

            for (int i = 0; i < popSize; i++)
            {
                var nest = new Dictionary<ClassInputModel, TeacherInputModel>();
                foreach (var classInfo in classInfoList)
                {
                    // Gán ngẫu nhiên một giảng viên cho lớp (hoặc để null nếu chưa phân công)
                    nest[classInfo] = random.Next(2) == 0 ? teacherInfoList[random.Next(teacherInfoList.Count)] : null;
                }
                nests.Add(nest);
            }
            return nests;
        }

        private Dictionary<ClassInputModel, TeacherInputModel> LevyFlight(
            Dictionary<ClassInputModel, TeacherInputModel> nest,
            List<ClassInputModel> classInfoList,
            List<TeacherInputModel> teacherInfoList)
        {
            var random = new Random();
            var newNest = new Dictionary<ClassInputModel, TeacherInputModel>(nest);

            // Thực hiện một số hoán đổi hoặc thay đổi ngẫu nhiên
            var classToChange = classInfoList[random.Next(classInfoList.Count)];
            var newTeacher = teacherInfoList[random.Next(teacherInfoList.Count)];

            newNest[classToChange] = newTeacher;
            return newNest;
        }

        private void ReplaceWorstNests(
            List<Dictionary<ClassInputModel, TeacherInputModel>> nests,
            double pa,
            List<ClassInputModel> classInfoList,
            List<TeacherInputModel> teacherInfoList)
        {
            var random = new Random();

            for (int i = 0; i < nests.Count * pa; i++)
            {
                var worstNestIndex = random.Next(nests.Count);
                nests[worstNestIndex] = InitializePopulation(classInfoList, teacherInfoList, 1)[0];
            }
        }

        private int EvaluateFitness(
            Dictionary<ClassInputModel, TeacherInputModel> nest,
            List<TeacherInputModel> teacherInfoList)
        {
            int fitness = 0;

            // Đánh giá dựa trên các ràng buộc
            foreach (var kvp in nest)
            {
                var classInfo = kvp.Key;
                var teacher = kvp.Value;

                if (teacher != null)
                {
                    // Ràng buộc: giảng viên thuộc nhóm chuyên môn
                    if (teacher.ListCourse.Any(course => course.Name == classInfo.CourseName))
                    {
                        fitness += 10;
                    }

                    // Ràng buộc: tổng số giờ dạy <= giới hạn
                    if (teacher.GdTeaching >= classInfo.GdTeaching)
                    {
                        fitness += 5;
                    }
                }
            }

            return fitness;
        }

        private SolutionModel ConvertNestToSolutionModel(Dictionary<ClassInputModel, TeacherInputModel> nest)
        {
            return new SolutionModel
            {
                ClassAssignments = nest.ToDictionary(
                    kvp => kvp.Key.Code,
                    kvp => kvp.Value?.Code // Giá trị có thể là null nếu vẫn không phân được
                )
            };
        }

        public async Task SaveTeachingAssignments(Task<SolutionModel> bestSolutionTask)
        {
            // Chờ để lấy kết quả từ Task<SolutionModel>
            var bestSolution = await bestSolutionTask;

            var teachingAssignments = new List<Data.CuckooTeachingAssignment>();

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
                var teachingAssignment = new Data.CuckooTeachingAssignment
                {
                    Id = Guid.NewGuid(),
                    Name = course.Name,
                    TeacherCode = teacherCode,
                    TeachingName = teacher.Name,
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
            await _cuckooTeachingRepository.AddRangeAsync(teachingAssignments);
        }

    }
}
