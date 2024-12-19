using TeachingAssignmentApp.Business.ETLGeneral.Model;
using TeachingAssignmentApp.Business.ProjectAssigment;
using TeachingAssignmentApp.Business.TeachingAssignment;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ETLGeneral
{
    public class GeneralService : IGeneralService
    {
        private readonly IGeneralRepository _generalRepository;
        private readonly IProjectAssignmentRepository _projectAssignmentRepository;
        private readonly ITeachingAssignmentRepository _teachingAssignmentRepository;
        public GeneralService(
            IGeneralRepository generalRepository,
            IProjectAssignmentRepository projectAssignmentRepository,
            ITeachingAssignmentRepository teachingAssignmentRepository
            )
        {
            _generalRepository = generalRepository;
            _projectAssignmentRepository = projectAssignmentRepository;
            _teachingAssignmentRepository = teachingAssignmentRepository;
        }

        public virtual async Task<IEnumerable<ETLGenteralResponse>> GetAllAync(string type)
        {
            var queryModel = new ETLGeneralQueryModel() { Type = type };

            var res = await _generalRepository.GetAllAync(queryModel);
            return res;
        }

        public virtual async Task<IEnumerable<Data.ETLGeneral>> RefreshAsync()
        {
            // Xóa hết dữ liệu cũ
            await _generalRepository.DeleteByTypeAsync(new List<string>()
            {
                 ETLGeneralTypeConstants.TotalStatistics,
                 ETLGeneralTypeConstants.PercentageAssignedWishes,
                 ETLGeneralTypeConstants.PercentageAssignedClasses,
            });
            // Cập nhật dữ liệu mới
            var generals = new List<Data.ETLGeneral>() { };
            generals.AddRange(await SaveTotalstatisticsAsync());
            generals.AddRange(await SaveStatisticClassAsync());
            generals.AddRange(await SaveStatisticAspirationAsync());
            return generals;
        }

        public async Task<IEnumerable<Data.ETLGeneral>> SaveStatisticClassAsync()
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var classNotAssignment = await _teachingAssignmentRepository.GetClassNotAssignmentAsync(queryProjectModel);
            var classes = await _generalRepository.ListAllClassAsync();

            var listTotalStatistics = new List<Data.ETLGeneral>() { };
            listTotalStatistics.AddRange(new List<Data.ETLGeneral>()
            {
                new Data.ETLGeneral()
                {
                    Label = "Số lượng lớp không được phân công",
                    Value = classNotAssignment.Content.Count(),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PercentageAssignedClasses
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng lớp học",
                    Value = classes.Count(),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PercentageAssignedClasses
                },
            });

            var res = await _generalRepository.SaveAsync(listTotalStatistics);
            return res;
        }

        public async Task<IEnumerable<Data.ETLGeneral>> SaveStatisticAspirationAsync()
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var projectNotAssignment = await _projectAssignmentRepository.GetProjectNotAssignmentAsync(queryProjectModel);
            var aspirations = await _generalRepository.ListAllAspirationAsync();

            var listTotalStatistics = new List<Data.ETLGeneral>() { };
            listTotalStatistics.AddRange(new List<Data.ETLGeneral>()
            {
                new Data.ETLGeneral()
                {
                    Label = "Số lượng đồ án không được phân công",
                    Value = projectNotAssignment.Content.Count(),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PercentageAssignedWishes
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng đồ án",
                    Value = aspirations.Count(),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PercentageAssignedWishes
                },
            });

            var res = await _generalRepository.SaveAsync(listTotalStatistics);
            return res;
        }

        public async Task<IEnumerable<Data.ETLGeneral>> SaveTotalstatisticsAsync()
        {
            var teachers = await _generalRepository.ListAllTeacherAsync();
            var professionalGroups = await _generalRepository.ListAllProfessionalGroupAsync();
            var classes = await _generalRepository.ListAllClassAsync();
            var aspirations = await _generalRepository.ListAllAspirationAsync();

            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var teacherNotAssignment = await _projectAssignmentRepository.GetTeacherNotAssignmentAsync(queryProjectModel);
            var projectNotAssignment = await _projectAssignmentRepository.GetProjectNotAssignmentAsync(queryProjectModel);
            var teacherNotClass = await _teachingAssignmentRepository.GetTeacherNotAssignmentAsync(queryProjectModel);
            var classNotAssignment = await _teachingAssignmentRepository.GetClassNotAssignmentAsync(queryProjectModel);

            var listTotalStatistics = new List<Data.ETLGeneral>() { };
            listTotalStatistics.AddRange(new List<Data.ETLGeneral>()
            {
                new Data.ETLGeneral()
                {
                    Label = "Số giảng viên cần phân công",
                    Value = teachers.Count(),
                    Category = "lecturer-management",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng nhóm chuyên môn",
                    Value = professionalGroups.Count(),
                    Category = "professional-group-management'",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng lớp học",
                    Value = classes.Count(),
                    Category = "class-management",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng đồ án cần phân công",
                    Value = aspirations.Count(),
                    Category = "aspiration-management",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                 new Data.ETLGeneral()
                {
                    Label = "Số lượng lớp không được phân công",
                    Value = classNotAssignment.Content.Count(),
                    Category = "teaching-not-assignment",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng GV không được PC giảng dạy",
                    Value = teacherNotClass.Content.Count(),
                    Category = "teacher-not-class",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng NV không được phân công",
                    Value = projectNotAssignment.Content.Count(),
                    Category = "project-not-assignment",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
                new Data.ETLGeneral()
                {
                    Label = "Số lượng GV không được PC hướng dẫn",
                    Value = teacherNotAssignment.Content.Count(),
                    Category = "teacher-not-aspiration",
                    Type = ETLGeneralTypeConstants.TotalStatistics
                },
            });

            var res = await _generalRepository.SaveAsync(listTotalStatistics);
            return res;
        }
    }
}
