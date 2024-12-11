using TeachingAssignmentApp.Business.ETLGeneral;
using TeachingAssignmentApp.Business.ETLGeneral.Model;
using TeachingAssignmentApp.Business.ETLTeacher.Model;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ETLTeacher
{
    public class TeacherETLService : ITeacherETLService
    {
        private readonly ITeacherETLRepository _teacherETLRepository;
        public TeacherETLService(ITeacherETLRepository teacherETLRepository)
        {
            _teacherETLRepository = teacherETLRepository;
        }

        public virtual async Task<IEnumerable<ETLGenteralResponse>> GetAllAync(ETLTeacherQueryModel eTLTeacherQueryModel)
        {
            var res = await _teacherETLRepository.GetAllAync(eTLTeacherQueryModel);
            return res;
        }

        public virtual async Task CreateAll()
        {
            for (int i = 2; i < 51; i++)
            {
                string role = $"GV{i:000}";
                await RefreshAsync(role);
            }
        }
        public virtual async Task<IEnumerable<Data.ETLTeacher>> RefreshAsync(string role)
        {
            // Xóa hết dữ liệu cũ
            await _teacherETLRepository.DeleteByTypeAsync(new List<string>()
            {
                 ETLGeneralTypeConstants.GDRatioAssigned,
                 ETLGeneralTypeConstants.TotalGDAnalyzed,
                 ETLGeneralTypeConstants.PerGdTeaching,
                 ETLGeneralTypeConstants.PerGdInstruct,
            }, role);
            // Cập nhật dữ liệu mới
            var generals = new List<Data.ETLTeacher>() { };
            generals.AddRange(await SaveGDRatioAsync(role));
            generals.AddRange(await SaveTotalGDAsync(role));
            generals.AddRange(await SavePerGdInstructAsync(role));
            generals.AddRange(await SavePerGdTeachingAsync(role));
            return generals;
        }

        public async Task<IEnumerable<Data.ETLTeacher>> SaveTotalGDAsync(string role)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var teachers = await _teacherETLRepository.ListAllTeacherAsync(role);
            var teacherRealTeaching = await _teacherETLRepository.ListAllGDRealAsync(role);
            var teacherRealProject = await _teacherETLRepository.ListAllGDProjectRealAsync(role);

            var listTotalStatistics = new List<Data.ETLTeacher>() { };
            listTotalStatistics.AddRange(new List<Data.ETLTeacher>()
            {
                new Data.ETLTeacher()
                {
                    Label = "GD tối đa",
                    Value = teachers.GdTeaching ?? 0.0,
                    Category = "GdTeaching",
                    Type = ETLGeneralTypeConstants.TotalGDAnalyzed
                },
                new Data.ETLTeacher()
                {
                    Label = "GD thực tế",
                    Value = teacherRealTeaching.Sum(t => t.GdTeaching ?? 0.0),
                    Category = "GdTeaching",
                    Type = ETLGeneralTypeConstants.TotalGDAnalyzed
                },
                new Data.ETLTeacher()
                {
                    Label = "GD tối đa",
                    Value = teachers.GdInstruct ?? 0.0,
                    Category = "GdInstruct",
                    Type = ETLGeneralTypeConstants.TotalGDAnalyzed
                },
                new Data.ETLTeacher()
                {
                    Label = "GD thực tế",
                    Value = teacherRealProject.Sum(t => t.GdInstruct) ?? 0.0,
                    Category = "GdInstruct",
                    Type = ETLGeneralTypeConstants.TotalGDAnalyzed
                },
            });

            var res = await _teacherETLRepository.SaveAsync(listTotalStatistics, role);
            return res;
        }

        public async Task<IEnumerable<Data.ETLTeacher>> SaveGDRatioAsync(string role)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var teacherRealTeaching = await _teacherETLRepository.ListAllGDRealAsync(role);
            var teacherRealProject = await _teacherETLRepository.ListAllGDProjectRealAsync(role);

            var listTotalStatistics = new List<Data.ETLTeacher>() { };
            listTotalStatistics.AddRange(new List<Data.ETLTeacher>()
            {
                new Data.ETLTeacher()
                {
                    Label = "GD giảng dạy được phân",
                    Value = teacherRealTeaching.Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.GDRatioAssigned
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hướng dẫn đồ án được phân",
                    Value = teacherRealProject.Sum(t => t.GdInstruct) ?? 0.0,
                    Category = "",
                    Type = ETLGeneralTypeConstants.GDRatioAssigned
                },
            });

            var res = await _teacherETLRepository.SaveAsync(listTotalStatistics, role);
            return res;
        }
        public async Task<IEnumerable<Data.ETLTeacher>> SavePerGdTeachingAsync(string role)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var teacherRealTeaching = await _teacherETLRepository.ListAllGDRealAsync(role);

            var listTotalStatistics = new List<Data.ETLTeacher>() { };
            listTotalStatistics.AddRange(new List<Data.ETLTeacher>()
            {
                new Data.ETLTeacher()
                {
                    Label = "GD hệ CTTT",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "CTTT").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ HEDSPI",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "HEDSPI").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ KSCQ",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "KSCQ").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ CN",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "CN").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ SIE",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "SIE").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ KSTN",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "KSTN").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ ThSKH",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "ThSKH").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
                new Data.ETLTeacher()
                {
                    Label = "GD hệ KSCLC",
                    Value = teacherRealTeaching.Where(t => t.GroupName == "KSCLC").Sum(t => t.GdTeaching ?? 0.0),
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdTeaching
                },
            });

            var res = await _teacherETLRepository.SaveAsync(listTotalStatistics, role);
            return res;
        }

        public async Task<IEnumerable<Data.ETLTeacher>> SavePerGdInstructAsync(string role)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
            };
            var teacherRealProject = await _teacherETLRepository.ListAllGDProjectRealAsync(role);
            var projects = await _teacherETLRepository.ListAllProjectAsync();

            var listTotalStatistics = new List<Data.ETLTeacher>() { };
            listTotalStatistics.AddRange(new List<Data.ETLTeacher>()
            {
                new Data.ETLTeacher()
                {
                    Label = "GD ĐAMH",
                    Value = teacherRealProject
                        .Where(trp => projects.Any(p =>
                            p.CourseName == trp.ClassName &&
                            p.Type == "ĐAMH"))
                        .Sum(trp => trp.GdInstruct) ?? 0.0,
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdInstruct
                },
                new Data.ETLTeacher()
                {
                    Label = "GD ĐATN",
                    Value = teacherRealProject
                        .Where(trp => projects.Any(p =>
                            p.CourseName == trp.ClassName &&
                            p.Type == "ĐATN"))
                        .Sum(trp => trp.GdInstruct) ?? 0.0,
                    Category = "",
                    Type = ETLGeneralTypeConstants.PerGdInstruct
                },
            });

            var res = await _teacherETLRepository.SaveAsync(listTotalStatistics, role);
            return res;
        }
    }
}
