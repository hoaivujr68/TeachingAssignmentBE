using LinqKit;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;
using System.Linq;
using TeachingAssignmentApp.Business.Teacher;
using TeachingAssignmentApp.Business.TeachingAssignment.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Text.RegularExpressions;

namespace TeachingAssignmentApp.Business.TeachingAssignment
{
    public class TeachingAssignmentRepository : ITeachingAssignmentRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        private readonly IClassRepository _classRepository;
        private readonly ITeacherRepository _teacherRepository;

        public TeachingAssignmentRepository(TeachingAssignmentDbContext context, IClassRepository classRepository, ITeacherRepository teacherRepository)
        {
            _context = context;
            _classRepository = classRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<Pagination<Data.TeachingAssignment>> GetAllAsync(QueryModel queryModel, string role)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.TeachingAssignment> query = BuildQuery(queryModel, role);

            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Pagination<ClassModel>> GetClassNotAssignmentAsync(QueryModel queryModel)
        {
            var queryClassModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 200,
                ListTextSearch = queryModel.ListTextSearch
            };

            var allClasses = await _classRepository.GetAllAsync(queryClassModel);

            var assignedClassCodes = await _context.TeachingAssignments
                                                    .Select(t => t.Code)
                                                    .ToListAsync();

            var classesNotAssigned = allClasses.Content
                                                .Where(c => !assignedClassCodes.Contains(c.Code))
                                                .ToList();

            var result = new Pagination<ClassModel>(
                classesNotAssigned,
                classesNotAssigned.Count,
                queryModel.CurrentPage ?? 1,
                queryModel.PageSize ?? 20
);

            return result;
        }
        public async Task<IEnumerable<TeacherModel>> GetAvailableTeachersForClass(string classId)
        {
            // 1. Lấy thông tin lớp học theo mã lớp
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Code == classId);

            if (classEntity == null)
            {
                throw new ArgumentException($"Class with ID {classId} not found.");
            }

            // 2. Lấy phân công hiện tại cho lớp học
            var currentAssignment = await _context.TeachingAssignments
                .FirstOrDefaultAsync(ta => ta.Code == classId);

            string currentTeacherCode = currentAssignment?.TeacherCode;
            var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Code == currentTeacherCode);

            // 3. Lấy danh sách giáo viên dạy môn học tương ứng
            var teachers = await _context.Teachers
                .Include(t => t.ListCourse)
                .Where(t => t.ListCourse != null && t.ListCourse.Any(c => c.Name == classEntity.CourseName)) // Kiểm tra trong ListCourse
                .ToListAsync();

            // 4. Lấy danh sách TeachingAssignments hiện tại
            var teachingAssignments = await _context.TeachingAssignments
                .Include(ta => ta.TimeTableDetail) // Bao gồm TimeTableDetail
                .ToListAsync();

            var availableTeachers = new List<TeacherModel>();

            foreach (var teacher in teachers)
            {
                // Bỏ qua kiểm tra nếu là giáo viên hiện tại
                if (teacher.Code == currentTeacherCode)
                {
                    availableTeachers.Add(new TeacherModel
                    {
                        Id = teacher.Id,
                        Code = teacher.Code,
                        Name = teacher.Name,
                        GdTeaching = teacher.GdTeaching,
                        GdInstruct = teacher.GdInstruct
                    });
                    continue;
                }

                // 4.1. Lấy danh sách phân công của giáo viên này
                var teacherAssignments = teachingAssignments.Where(ta => ta.TeacherCode == teacher.Code).ToList();

                // 4.2. Kiểm tra lịch học không trùng
                bool isScheduleConflict = teacherAssignments.Any(ta =>
                    ta.TimeTableDetail != null &&
                    ta.TimeTableDetail.Any(assignedSlot =>
                        classEntity.TimeTableDetail.Any(classSlot =>
                            assignedSlot.Day == classSlot.Day &&
                            assignedSlot.Seasion == classSlot.Seasion &&
                            assignedSlot.Period.Intersect(classSlot.Period).Any())));

                if (isScheduleConflict)
                {
                    continue; // Bỏ qua giáo viên nếu lịch trùng
                }

                // 4.3. Kiểm tra tổng giờ giảng không vượt quá giới hạn
                double totalTeachingHours = teacherAssignments.Sum(ta => ta.GdTeaching ?? 0) + classEntity.GdTeaching;
                if (totalTeachingHours > (teacher.GdTeaching ?? 0) * 2)
                {
                    continue; // Bỏ qua giáo viên nếu giờ giảng vượt giới hạn
                }

                // 4.4. Thêm vào danh sách giáo viên khả dụng
                availableTeachers.Add(new TeacherModel
                {
                    Id = teacher.Id,
                    Code = teacher.Code,
                    Name = teacher.Name,
                    GdTeaching = teacher.GdTeaching,
                    GdInstruct = teacher.GdInstruct // (Tùy chỉnh giá trị này nếu cần)
                });
            }

            if (!availableTeachers.Any(t => t.Code == currentTeacher.Code))
            {
                availableTeachers.Add(new TeacherModel
                {
                    Id = currentTeacher.Id,
                    Code = currentTeacher.Code,
                    Name = currentTeacher.Name,
                    GdTeaching = currentTeacher.GdTeaching,
                    GdInstruct = currentTeacher.GdInstruct
                });
            }

            return availableTeachers;
        }

        public async Task<double> GetTotalGdTeachingByTeacherCode(string teacherCode)
        {
            return await _context.TeachingAssignments
                .Where(ta => ta.TeacherCode == teacherCode)
                .SumAsync(ta => ta.GdTeaching ?? 0.0);
        }

        public async Task<Pagination<TeacherModel>> GetTeacherNotAssignmentAsync(QueryModel queryModel)
        {
            var queryClassModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 200,
                ListTextSearch = queryModel.ListTextSearch
            };

            var allClasses = await _teacherRepository.GetAllAsync(queryClassModel);

            var assignedTeacherCodes = await _context.TeachingAssignments
                                             .Select(t => t.TeacherCode)
                                             .ToListAsync();

            var teachersNotAssigned = allClasses.Content
                                                .Where(c => !assignedTeacherCodes.Contains(c.Code))
                                                .ToList();

            var result = new Pagination<TeacherModel>(
                teachersNotAssigned,
                teachersNotAssigned.Count,
                queryModel.CurrentPage ?? 1,
                queryModel.PageSize ?? 20
);

            return result;
        }

        public async Task<Data.TeachingAssignment> GetByIdAsync(Guid id)
        {
            return await _context.TeachingAssignments.FindAsync(id);
        }

        public async Task AddAsync(Data.TeachingAssignment teachingAssignment)
        {
            if (teachingAssignment.Id == Guid.Empty)
            {
                teachingAssignment.Id = Guid.NewGuid();
            }

            await _context.TeachingAssignments.AddAsync(teachingAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task<Data.TeachingAssignment> UpdateAsync(Guid id, Data.TeachingAssignment updatedTeachingAssignment)
        {
            var existingAssignment = await _context.TeachingAssignments
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingAssignment == null)
            {
                throw new KeyNotFoundException($"TeachingAssignment với Id '{id}' không tồn tại.");
            }

            var teacher = await _teacherRepository.GetByCodeAsync(updatedTeachingAssignment.TeacherCode);
            if (teacher == null)
            {
                throw new KeyNotFoundException($"Teacher với Code '{updatedTeachingAssignment.TeacherCode}' không tồn tại.");
            }
            // Cập nhật các trường từ updatedTeachingAssignment
            existingAssignment.Name = updatedTeachingAssignment.Name ?? existingAssignment.Name;
            existingAssignment.Code = updatedTeachingAssignment.Code ?? existingAssignment.Code;
            existingAssignment.Type = updatedTeachingAssignment.Type ?? existingAssignment.Type;
            existingAssignment.CourseName = updatedTeachingAssignment.CourseName ?? existingAssignment.CourseName;
            existingAssignment.GroupName = updatedTeachingAssignment.GroupName ?? existingAssignment.GroupName;
            existingAssignment.MaxEnrol = updatedTeachingAssignment.MaxEnrol != 0
                ? updatedTeachingAssignment.MaxEnrol
                : existingAssignment.MaxEnrol;
            existingAssignment.TimeTable = updatedTeachingAssignment.TimeTable ?? existingAssignment.TimeTable;
            existingAssignment.GdTeaching = updatedTeachingAssignment.GdTeaching != 0
                ? updatedTeachingAssignment.GdTeaching
                : existingAssignment.GdTeaching;
            existingAssignment.TeacherCode = updatedTeachingAssignment.TeacherCode ?? existingAssignment.TeacherCode;
            existingAssignment.TeachingName = teacher.Name;

            // Cập nhật các trường phức tạp (nếu cần)
            if (updatedTeachingAssignment.TimeTableDetail != null)
            {
                existingAssignment.TimeTableDetail = updatedTeachingAssignment.TimeTableDetail;
            }

            // Lưu thay đổi vào database
            _context.TeachingAssignments.Update(existingAssignment);
            await _context.SaveChangesAsync();
            return existingAssignment;
        }

        public async Task<double?> GetRangeGdTeaching()
        {
            var totalGdTeaching = await _context.Teachers
                .SumAsync(ta => ta.GdTeaching ?? 0.0);
            var totalGd = await _context.Classes
                .SumAsync(ta => ta.GdTeaching);
            totalGd = Math.Round(totalGd, 2);
            return Math.Round(totalGd / totalGdTeaching, 2);
        }

        public async Task DeleteAsync(Guid id)
        {
            var teachingAssignment = await _context.TeachingAssignments.FindAsync(id);
            if (teachingAssignment != null)
            {
                _context.TeachingAssignments.Remove(teachingAssignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<byte[]> ExportTeacherAssignmentByQuota()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thống kê phân công");
                var teacherAssignments = await _context.TeachingAssignments.ToListAsync();
                worksheet.Cells[1, 5].Value = "Chưa phân công";
                worksheet.Cells[2, 5].Value = "Tổng";
                worksheet.Cells[3, 5].Value = "Đã phân công";
                worksheet.Cells[4, 5].Value = "GD loại môn học";
                worksheet.Cells[5, 5].Value = "Quota max (1.5 trung bình)";
                worksheet.Cells[6, 5].Value = "Loại môn học";
                worksheet.Cells[1, 6].Value = 0;
                worksheet.Cells[2, 6].Value = teacherAssignments.Count;
                worksheet.Cells[3, 6].Value = teacherAssignments.Count;
                worksheet.Cells[7, 1].Value = "STT";
                worksheet.Cells[7, 2].Value = "Giảng viên";
                worksheet.Cells[7, 3].Value = "GD được phân";
                worksheet.Cells[7, 4].Value = "Tổng phân công";

                var groupedAssignments = teacherAssignments
                    .GroupBy(p => new { p.CourseName, p.GdTeaching })
                    .Select(group => new
                    {
                        CourseName = group.Key.CourseName,
                        GdTeaching = group.Key.GdTeaching,
                        NameAndGroups = string.Join(", ", group
                            .GroupBy(p => p.Name)
                            .Select(subGroup => $"{string.Join(", ", subGroup.Select(p => $"{p.CourseName} {p.Name} {p.GroupName}").Distinct())}") // Gắn ClassName vào GroupName
                        ),
                        TotalCount = group.Count(), // Tổng số bản ghi trong nhóm
                        QuotaRange = Math.Ceiling(group.Count() / 49.0 * 1.5)
                    })
                    .OrderBy(g => g.GdTeaching) // Sắp xếp theo GdInstruct
                    .ThenBy(g => g.CourseName)   // Sau đó sắp xếp theo ClassName
                    .ToList();

                var groupedAssignmentTeachers = teacherAssignments
                    .GroupBy(p => new { p.TeacherCode, p.TeachingName })
                    .Select(teacherGroup => new
                    {
                        TeacherCode = teacherGroup.Key.TeacherCode,
                        TeachingName = teacherGroup.Key.TeachingName,
                        GdInstruct = teacherGroup.Sum(t => t.GdTeaching), // Tổng GdInstruct của tất cả bản ghi của giảng viên
                        Assignments = teacherGroup
                            .GroupBy(p => new { p.CourseName, p.GdTeaching })
                            .Select(classGroup => new
                            {
                                CourseName = classGroup.Key.CourseName,
                                GdTeaching = classGroup.Key.GdTeaching,
                                TotalCount = classGroup.Count(),
                                NameAndGroups = string.Join(", ", classGroup
                                    .Select(p => $"{p.CourseName} {p.Name} {p.GroupName}")
                                    .Distinct())
                            })
                            .ToList()
                    })
                    .ToList();

                var groupedAssignmentGroups = teacherAssignments
                   .GroupBy(p => new { p.GroupName, p.GdTeaching })
                   .Select(group => new
                   {
                       GroupName = group.Key.GroupName,
                       GdTeaching = group.Key.GdTeaching,
                       NameAndGroups = string.Join(", ", group
                           .GroupBy(p => p.Name)
                           .Select(subGroup => $"{string.Join(", ", subGroup.Select(p => $"{p.GroupName} {p.Name}").Distinct())}") // Gắn ClassName vào GroupName
                       ),
                       TotalCount = group.Count(), // Tổng số bản ghi trong nhóm
                       QuotaRange = Math.Ceiling(group.Count() / 49.0 * 1.5)
                   })
                   .OrderBy(g => g.GdTeaching) // Sắp xếp theo GdInstruct
                   .ThenBy(g => g.GroupName)   // Sau đó sắp xếp theo ClassName
                   .ToList();

                var groupedAssignmentTeacherGroups = teacherAssignments
                    .GroupBy(p => new { p.TeacherCode, p.TeachingName })
                    .Select(teacherGroup => new
                    {
                        TeacherCode = teacherGroup.Key.TeacherCode,
                        TeachingName = teacherGroup.Key.TeachingName,
                        GdTeaching = teacherGroup.Sum(t => t.GdTeaching), // Tổng GdInstruct của tất cả bản ghi của giảng viên
                        Assignments = teacherGroup
                            .GroupBy(p => new { p.GroupName, p.GdTeaching })
                            .Select(classGroup => new
                            {
                                GroupName = classGroup.Key.GroupName,
                                GdTeaching = classGroup.Key.GdTeaching,
                                TotalCount = classGroup.Count(),
                                NameAndGroups = string.Join(", ", classGroup
                                    .Select(p => $"{p.Name}")
                                    .Distinct())
                            })
                            .ToList()
                    })
                    .ToList();

                for (int i = 0; i < groupedAssignmentTeachers.Count; i++)
                {
                    var teacherGroup = groupedAssignmentTeachers[i];
                    worksheet.Cells[i + 8, 1].Value = i + 1;
                    worksheet.Cells[i + 8, 2].Value = teacherGroup.TeachingName; // Tên giảng viên
                    worksheet.Cells[i + 8, 3].Value = Math.Round(teacherGroup.GdInstruct ?? 0.0, 2); // Tổng GdInstruct
                    worksheet.Cells[i + 8, 4].Value = teacherGroup.Assignments.Sum(p => p.TotalCount); // Tổng GdInstruct

                    for (int j = 0; j < groupedAssignments.Count; j++)
                    {
                        var assignment = groupedAssignments[j];

                        // Kiểm tra nếu ClassName và GdInstruct của giảng viên phù hợp với nhóm trong groupedAssignments
                        var matchedAssignment = teacherGroup.Assignments
                            .FirstOrDefault(a => a.CourseName == assignment.CourseName && a.GdTeaching == assignment.GdTeaching);

                        var cell = worksheet.Cells[i + 8, j + 7]; // Xác định ô hiện tại
                        var totalCount = matchedAssignment?.TotalCount ?? 0; // Lấy giá trị TotalCount
                        cell.Value = totalCount;

                        // Tô màu nền ô nếu vượt quá QuotaRange
                        if (totalCount > assignment.QuotaRange)
                        {
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red); // Màu nền đỏ
                        }
                    }
                }

                for (int i = 0; i < groupedAssignments.Count; i++)
                {
                    worksheet.Cells[1, i + 7].Value = 0;
                    worksheet.Cells[2, i + 7].Value = groupedAssignments[i].TotalCount;
                    worksheet.Cells[3, i + 7].Value = groupedAssignments[i].TotalCount;
                    worksheet.Cells[4, i + 7].Value = groupedAssignments[i].GdTeaching;
                    worksheet.Cells[5, i + 7].Value = groupedAssignments[i].QuotaRange;
                    worksheet.Cells[6, i + 7].Value = groupedAssignments[i].NameAndGroups;
                }
                var groupedAssignmentsCount = groupedAssignments.Count;
                worksheet.Cells[4, 8 + groupedAssignmentsCount].Value = "Thống kê theo hệ đào tạo";
                worksheet.Cells[1, 9 + groupedAssignmentsCount].Value = 0;
                worksheet.Cells[2, 9 + groupedAssignmentsCount].Value = teacherAssignments.Count;
                worksheet.Cells[3, 9 + groupedAssignmentsCount].Value = teacherAssignments.Count;
                for (int i = 0; i < groupedAssignmentGroups.Count; i++)
                {
                    worksheet.Cells[1, i + 10 + groupedAssignmentsCount].Value = 0;
                    worksheet.Cells[2, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].TotalCount;
                    worksheet.Cells[3, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].TotalCount;
                    worksheet.Cells[4, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].GdTeaching;
                    worksheet.Cells[5, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].QuotaRange;
                    worksheet.Cells[6, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].NameAndGroups;
                }

                for (int i = 0; i < groupedAssignmentTeacherGroups.Count; i++)
                {
                    var teacherGroup = groupedAssignmentTeacherGroups[i];

                    for (int j = 0; j < groupedAssignmentGroups.Count; j++)
                    {
                        var assignment = groupedAssignmentGroups[j];

                        // Kiểm tra nếu ClassName và GdInstruct của giảng viên phù hợp với nhóm trong groupedAssignments
                        var matchedAssignment = teacherGroup.Assignments
                            .FirstOrDefault(a => a.GroupName == assignment.GroupName && a.GdTeaching == assignment.GdTeaching);

                        var cell = worksheet.Cells[i + 8, j + 10 + groupedAssignmentsCount]; // Xác định ô hiện tại
                        var totalCount = matchedAssignment?.TotalCount ?? 0; // Lấy giá trị TotalCount
                        cell.Value = totalCount;

                        // Tô màu nền ô nếu vượt quá QuotaRange
                        if (totalCount > assignment.QuotaRange)
                        {
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red); // Màu nền đỏ
                        }
                    }
                }

                // Cố định cột A và B
                worksheet.View.FreezePanes(8, 3); // Cố định từ hàng 8 và cột 3 (cột C)
                worksheet.Column(1).AutoFit(); // Cột A
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                return package.GetAsByteArray();
            }
        }

        public async Task<byte[]> ExportTeachingAssignment(string role)
        {
            IQueryable<Data.TeachingAssignment> query;

            if (role == "lanhdao" || role == "admin")
            {
                query = _context.TeachingAssignments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải lanhdao hoặc admin
                query = _context.TeachingAssignments.Where(pa => pa.TeacherCode == role);
            }

            // Sắp xếp và lấy dữ liệu
            var teachingAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (teachingAssignments == null || teachingAssignments.Count == 0)
            {
                throw new InvalidOperationException("No TeachingAssignments available to export.");
            }

            var teachers = await _context.Teachers.ToListAsync();

            // Nhóm phân công theo TeacherCode và tính tổng GdTeaching của từng giảng viên
            var groupedAssignments = teachingAssignments
                .GroupBy(pa => pa.TeacherCode)
                .Select(g => new
                {
                    TeacherCode = g.Key,
                    TotalGdTeaching = g.Sum(pa => pa.GdTeaching),
                    Assignments = g.ToList()
                })
                .OrderBy(g => g.TeacherCode)
                .ToList();

            // Tạo dictionary để tra cứu GdTeaching của giảng viên nhanh hơn
            var teacherDictionary = teachers.ToDictionary(
                t => t.Code,                // Khóa: TeacherCode
                t => new Tuple<string, double?>(t.Name, t.GdTeaching));

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phân công giảng dạy");

                // Thêm tiêu đề cột
                worksheet.Cells[1, 1].Value = "Mã giảng viên";
                worksheet.Cells[1, 2].Value = "Tên giảng viên";
                worksheet.Cells[1, 3].Value = "Mã lớp";
                worksheet.Cells[1, 4].Value = "Mã môn học";
                worksheet.Cells[1, 5].Value = "Tên môn học";
                worksheet.Cells[1, 6].Value = "Loại môn học";
                worksheet.Cells[1, 7].Value = "Hệ";
                worksheet.Cells[1, 8].Value = "Số lượng sinh viên max";
                worksheet.Cells[1, 9].Value = "Lịch học";
                worksheet.Cells[1, 10].Value = "Gd môn học";
                worksheet.Cells[1, 11].Value = "Gd giảng viên";
                worksheet.Cells[1, 12].Value = "Tổng Gd phân công theo giảng viên";
                worksheet.Cells[1, 13].Value = "Tỷ lệ";
                worksheet.Cells[1, 14].Value = "Một thời điểm - Một lớp";
                worksheet.Cells[1, 15].Value = "Đúng chuyên môn";
                worksheet.Cells[1, 16].Value = "Số giờ phân công thỏa mãn";
                worksheet.Cells[1, 17].Value = "Giờ giảng dạy cân bằng";
                worksheet.Cells[1, 18].Value = "Cùng ngày học";

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 18].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 18].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 18].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                int row = 2;
                foreach (var group in groupedAssignments)
                {
                    // Thêm thông tin giảng viên
                    worksheet.Cells[row, 1].Value = group.TeacherCode;
                    worksheet.Cells[row, 12].Value = group.TotalGdTeaching;

                    var test = group.Assignments.ToList();
                    if (test.Any(a => a.TimeTable == "HV liên hệ với giáo viên") || test.Count == 1)
                    {
                        worksheet.Cells[row, 18].Value = "Thỏa mãn";
                    }
                    else
                    {
                        // Danh sách chứa các ngày từ TimeTable
                        var daysInSchedules = new List<string>();

                        foreach (var assignment in test)
                        {
                            var timeTable = assignment.TimeTable;

                            // Tách TimeTable thành các phần, ví dụ: Sáng T5, Chiều T5
                            var schedules = timeTable.Split(new[] { "Sáng", "Chiều" }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var schedule in schedules)
                            {
                                string session = timeTable.Contains("Sáng" + schedule) ? "Sáng" : "Chiều";
                                var parts = schedule.Split(',');

                                // Lấy ngày từ phần TimeTable
                                string day = parts[0].Split('T')[1].Trim().Replace(":", "").Trim();

                                // Thêm ngày vào danh sách
                                if (!string.IsNullOrEmpty(day))
                                {
                                    daysInSchedules.Add(day);
                                }
                            }
                        }

                        // Kiểm tra sự trùng lặp ngày
                        var duplicateDays = daysInSchedules
                            .GroupBy(d => d) // Nhóm theo ngày
                            .Where(g => g.Count() > 1) // Kiểm tra nếu một ngày xuất hiện nhiều lần
                            .Select(g => g.Key)
                            .ToList();

                        // Nếu có ngày trùng, gán "Thỏa mãn"
                        if (duplicateDays.Any())
                        {
                            worksheet.Cells[row, 18].Value = "Thỏa mãn";
                        }
                        else
                        {
                            worksheet.Cells[row, 18].Value = "Không thỏa mãn";
                        }
                    }
                    // Tìm GdTeaching của giảng viên trong dictionary
                    if (teacherDictionary.TryGetValue(group.TeacherCode, out var teacherInfo))
                    {
                        worksheet.Cells[row, 2].Value = teacherInfo.Item1;
                        worksheet.Cells[row, 11].Value = teacherInfo.Item2;
                        worksheet.Cells[row, 13].Value = Math.Round((double)(group.TotalGdTeaching) / (double)(teacherInfo.Item2 ?? 0), 2).ToString();
                        var rate = Math.Round((double)(group.TotalGdTeaching) / (double)(teacherInfo.Item2 ?? 0), 2);
                        if (rate <= 1.6 && rate >= 1.3) worksheet.Cells[row, 17].Value = "Thỏa mãn";
                        else worksheet.Cells[row, 17].Value = "Không thỏa mãn";
                        if (rate <= 1.5) worksheet.Cells[row, 16].Value = "Thỏa mãn";
                        else worksheet.Cells[row, 16].Value = "Không thỏa mãn";
                    }
                    worksheet.Cells[row, 14].Value = "Thỏa mãn";
                    worksheet.Cells[row, 15].Value = "Thỏa mãn";

                    // Thêm dữ liệu phân công của từng giảng viên
                    foreach (var assignment in group.Assignments)
                    {
                        worksheet.Cells[row, 3].Value = assignment.Code;
                        worksheet.Cells[row, 4].Value = assignment.CourseName;
                        worksheet.Cells[row, 5].Value = assignment.Name;
                        worksheet.Cells[row, 6].Value = assignment.Type;
                        worksheet.Cells[row, 7].Value = assignment.GroupName;
                        worksheet.Cells[row, 8].Value = assignment.MaxEnrol;
                        worksheet.Cells[row, 9].Value = assignment.TimeTable;
                        worksheet.Cells[row, 10].Value = assignment.GdTeaching;

                        row++; // Di chuyển xuống dòng tiếp theo
                    }
                }

                // Tự động căn chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Trả về file Excel dưới dạng byte array
                return package.GetAsByteArray();
            }
        }

        public async Task<IEnumerable<ResultModel>> GetResultModel()
        {
            var teachers = await _context.Teachers.ToListAsync();
            var classes = await _context.Classes.ToListAsync();
            var professor = await _context.ProfessionalGroups.ToListAsync();

            var totalGdTeaching = await _context.Teachers
              .SumAsync(ta => ta.GdTeaching ?? 0.0);
            var totalGd = await _context.Classes
                .SumAsync(ta => ta.GdTeaching);
            totalGd = Math.Round(totalGd, 2);
            totalGdTeaching = Math.Round(totalGdTeaching, 2);
            var rateGD = Math.Round(totalGd / totalGdTeaching, 2);

            IQueryable<Data.TeachingAssignment> query = _context.TeachingAssignments;

            var teachingAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (teachingAssignments == null || teachingAssignments.Count == 0)
            {
                throw new InvalidOperationException("No TeachingAssignments available to export.");
            }
            var groupedAssignments = teachingAssignments
                .GroupBy(pa => pa.TeacherCode)
                .Select(g => new
                {
                    TeacherCode = g.Key,
                    TotalGdTeaching = g.Sum(pa => pa.GdTeaching),
                    Assignments = g.ToList()
                })
                .OrderBy(g => g.TeacherCode)
                .ToList();

            // Tạo dictionary để tra cứu GdTeaching của giảng viên nhanh hơn
            var teacherDictionary = teachers.ToDictionary(
                t => t.Code,                // Khóa: TeacherCode
                t => new Tuple<string, double?>(t.Name, t.GdTeaching));

            var teacherCountRB7 = 0;
            var teacherCountRB8 = 0;
            var teacherCountRB5 = 0;

            foreach (var group in groupedAssignments)
            {
                if (teacherDictionary.TryGetValue(group.TeacherCode, out var teacherInfo))
                {
                    var rate = Math.Round((double)(group.TotalGdTeaching) / (double)(teacherInfo.Item2 ?? 0), 2);
                    if (rate <= 1.5 && rate >= 1.2) teacherCountRB8 += 1;
                    if (rate <= 1.5) teacherCountRB5 += 1;
                }

                var test = group.Assignments.ToList();
                if (test.Any(a => a.TimeTable == "HV liên hệ với giáo viên") || test.Count == 1)
                {
                    teacherCountRB7 += 1;
                    continue;
                }
                else
                {
                    // Danh sách chứa các ngày từ TimeTable
                    var daysInSchedules = new List<string>();

                    foreach (var assignment in test)
                    {
                        var timeTable = assignment.TimeTable;

                        // Tách TimeTable thành các phần, ví dụ: Sáng T5, Chiều T5
                        var schedules = timeTable.Split(new[] { "Sáng", "Chiều" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var schedule in schedules)
                        {
                            string session = timeTable.Contains("Sáng" + schedule) ? "Sáng" : "Chiều";
                            var parts = schedule.Split(',');

                            // Lấy ngày từ phần TimeTable
                            string day = parts[0].Split('T')[1].Trim().Replace(":", "").Trim();

                            // Thêm ngày vào danh sách
                            if (!string.IsNullOrEmpty(day))
                            {
                                daysInSchedules.Add(day);
                            }
                        }
                    }

                    // Kiểm tra sự trùng lặp ngày
                    var duplicateDays = daysInSchedules
                        .GroupBy(d => d) // Nhóm theo ngày
                        .Where(g => g.Count() > 1) // Kiểm tra nếu một ngày xuất hiện nhiều lần
                        .Select(g => g.Key)
                        .ToList();

                    // Nếu có ngày trùng, gán "Thỏa mãn"
                    if (duplicateDays.Any())
                    {
                        teacherCountRB7 += 1;
                    }
                }
            }

            var result = new List<ResultModel>
            {
                new ResultModel
                {
                    Label = "Một lớp - Một giáo viên",
                    Value = classes.Count,
                    Code = "RB1",
                    Type = "Table",
                    Category = "lớp học"
                },
                new ResultModel
                {
                    Label = "Một thời điểm - Một lớp",
                    Value = teachers.Count,
                    Code = "RB2",
                    Type = "Table",
                    Category = "giảng viên"
                },
                new ResultModel
                {
                    Label = "Đúng chuyên môn",
                    Value = teachers.Count,
                    Code = "RB3",
                    Type = "Table",
                    Category = "giảng viên"
                },
                new ResultModel
                {
                    Label = "Số giờ phân công thỏa mãn",
                    Value = teacherCountRB5,
                    Code = "RB5",
                    Type = "Table",
                    Category = "giảng viên"
                },
                new ResultModel
                {
                    Label = "Cùng ngày học",
                    Value = teacherCountRB7,
                    Code = "RB7",
                    Type = "Table",
                    Category = "giảng viên"
                },
                new ResultModel
                {
                    Label = "Giờ giảng dạy cân bằng",
                    Value = teacherCountRB8,
                    Code = "RB8",
                    Type = "Table",
                    Category = "giảng viên"
                },
                 new ResultModel
                {
                    Label = "Số giảng viên",
                    Value = teachers.Count,
                    Code = "TC",
                    Type = "Header",
                    Category = "giảng viên"
                },
                  new ResultModel
                {
                    Label = "Số lớp",
                    Value = classes.Count,
                    Code = "CL",
                    Type = "Header",
                    Category = "giảng viên"
                },
                   new ResultModel
                {
                    Label = "Số nhóm chuyên môn",
                    Value = professor.Count,
                    Code = "NCM",
                    Type = "Header",
                    Category = "giảng viên"
                }
                   ,
                   new ResultModel
                {
                    Label = "Tổng GD giảng viên",
                    Value = totalGdTeaching,
                    Code = "totalGdTeaching",
                    Type = "Header",
                    Category = "giảng viên"
                }
                   ,
                   new ResultModel
                {
                    Label = "Tổng GD lớp học",
                    Value = totalGd,
                    Code = "totalGd",
                    Type = "Header",
                    Category = "giảng viên"
                }
                   ,
                   new ResultModel
                {
                    Label = "Tỷ lệ trung bình",
                    Value = rateGD,
                    Code = "rateGD",
                    Type = "Header",
                    Category = "giảng viên"
                }
            };
            return result;
        }

        public async Task SwapTeacherAssignmentAsync(Guid teacherAssignmentId1, Guid teacherAssignmentId2)
        {
            var teacherAssignment1 = await _context.TeachingAssignments.FindAsync(teacherAssignmentId1);
            var teacherAssignment2 = await _context.TeachingAssignments.FindAsync(teacherAssignmentId2);

            if (teacherAssignment1 == null || teacherAssignment2 == null)
            {
                throw new InvalidOperationException("One or both teacher assignments could not be found.");
            }

            // Lưu tạm giá trị của TeacherCode từ teacherAssignment1
            var tempTeacherCode = teacherAssignment1.TeacherCode;
            var tempTeacherName = teacherAssignment1.TeachingName;

            // Hoán đổi TeacherCode giữa hai bản ghi
            teacherAssignment1.TeacherCode = teacherAssignment2.TeacherCode;
            teacherAssignment1.TeachingName = teacherAssignment2.TeachingName;
            teacherAssignment2.TeacherCode = tempTeacherCode;
            teacherAssignment2.TeachingName = tempTeacherName;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> ExportClassAssignment(string role)
        {
            IQueryable<Data.TeachingAssignment> query;

            if (role == "lanhdao" || role == "admin")
            {
                query = _context.TeachingAssignments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải lanhdao hoặc admin
                query = _context.TeachingAssignments.Where(pa => pa.TeacherCode == role);
            }

            // Sắp xếp và lấy dữ liệu
            var teachingAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (teachingAssignments == null || teachingAssignments.Count == 0)
            {
                throw new InvalidOperationException("No TeachingAssignments available to export.");
            }

            var teachers = await _context.Teachers.ToListAsync();

            // Nhóm phân công theo TeacherCode và tính tổng GdTeaching của từng giảng viên
            var groupedAssignments = teachingAssignments
                .GroupBy(pa => pa.TeacherCode)
                .Select(g => new
                {
                    TeacherCode = g.Key,
                    TotalGdTeaching = g.Sum(pa => pa.GdTeaching),
                    Assignments = g.ToList()
                })
                .OrderBy(g => g.TeacherCode)
                .ToList();

            // Tạo dictionary để tra cứu GdTeaching của giảng viên nhanh hơn
            var teacherDictionary = teachers.ToDictionary(
                t => t.Code,                // Khóa: TeacherCode
                t => new Tuple<string, double?>(t.Name, t.GdTeaching));

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phân công giảng dạy");

                // Thêm tiêu đề cột
                worksheet.Cells[1, 1].Value = "Mã lớp";
                worksheet.Cells[1, 2].Value = "Mã môn học";
                worksheet.Cells[1, 3].Value = "Tên môn học";
                worksheet.Cells[1, 4].Value = "Loại môn học";
                worksheet.Cells[1, 5].Value = "Hệ";
                worksheet.Cells[1, 6].Value = "Số lượng sinh viên max";
                worksheet.Cells[1, 7].Value = "Lịch học";
                worksheet.Cells[1, 8].Value = "Gd môn học";
                worksheet.Cells[1, 9].Value = "Mã giảng viên";
                worksheet.Cells[1, 10].Value = "Tên giảng viên";
                worksheet.Cells[1, 11].Value = "Gd giảng viên";
                worksheet.Cells[1, 12].Value = "Tổng Gd phân công theo giảng viên";
                worksheet.Cells[1, 13].Value = "Tỷ lệ";
                worksheet.Cells[1, 14].Value = "Một lớp - Một giáo viên";

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 14].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 14].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                int row = 2;
                foreach (var group in groupedAssignments)
                {
                    // Thêm thông tin giảng viên
                    worksheet.Cells[row, 9].Value = group.TeacherCode;
                    worksheet.Cells[row, 12].Value = group.TotalGdTeaching;

                    // Tìm GdTeaching của giảng viên trong dictionary
                    if (teacherDictionary.TryGetValue(group.TeacherCode, out var teacherInfo))
                    {
                        worksheet.Cells[row, 10].Value = teacherInfo.Item1;
                        worksheet.Cells[row, 11].Value = teacherInfo.Item2;
                        worksheet.Cells[row, 13].Value = Math.Round((double)(group.TotalGdTeaching) / (double)(teacherInfo.Item2 ?? 0), 2).ToString();

                    }
                    worksheet.Cells[row, 14].Value = "Thỏa mãn";

                    // Thêm dữ liệu phân công của từng giảng viên
                    foreach (var assignment in group.Assignments)
                    {
                        worksheet.Cells[row, 1].Value = assignment.Code;
                        worksheet.Cells[row, 2].Value = assignment.CourseName;
                        worksheet.Cells[row, 3].Value = assignment.Name;
                        worksheet.Cells[row, 4].Value = assignment.Type;
                        worksheet.Cells[row, 5].Value = assignment.GroupName;
                        worksheet.Cells[row, 6].Value = assignment.MaxEnrol;
                        worksheet.Cells[row, 7].Value = assignment.TimeTable;
                        worksheet.Cells[row, 8].Value = assignment.GdTeaching;

                        row++; // Di chuyển xuống dòng tiếp theo
                    }
                }

                // Tự động căn chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Trả về file Excel dưới dạng byte array
                return package.GetAsByteArray();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.TeachingAssignment> teachingAssignments)
        {
            await _context.TeachingAssignments.AddRangeAsync(teachingAssignments);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TimeTableResponse>> GetTimeTableByRole(string role)
        {
            // Khung giờ tiết học
            var timeSlotsStart = new Dictionary<int, string>
    {
        { 1, "6h45" }, { 2, "7h30" }, { 3, "8h25" }, { 4, "9h20" },
        { 5, "10h15" }, { 6, "11h00" }, { 7, "12h30" }, { 8, "13h15" },
        { 9, "14h10" }, { 10, "15h05" }, { 11, "16h00" }, { 12, "16h45" },
        { 13, "17h45" }, { 14, "18h30" }
    };

            var timeSlotsEnd = new Dictionary<int, string>
    {
        { 1, "7h30" }, { 2, "8h15" }, { 3, "9h10" }, { 4, "10h05" },
        { 5, "11h00" }, { 6, "11h45" }, { 7, "13h15" }, { 8, "14h00" },
        { 9, "14h55" }, { 10, "15h50" }, { 11, "16h45" }, { 12, "17h30" },
        { 13, "18h30" }, { 14, "19h15" }
    };

            // Lấy thông tin phân công giảng dạy
            var teachingAssignments = await _context.TeachingAssignments
                .Where(p => p.TeacherCode == role)
                .ToListAsync();

            var result = new List<TimeTableResponse>();

            // Xử lý từng phân công
            foreach (var teachingAssignment in teachingAssignments)
            {
                // Tách các dòng trong TimeTable
                var timetableEntries = teachingAssignment.TimeTable.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in timetableEntries)
                {
                    // Sử dụng Regex để tách thông tin
                    var match = Regex.Match(entry, @"(\S+)\s(T\d+):\sTiết\s(\d+)-(\d+)\s*,\s*Địa\sđiểm:\s([\w\-]+)");

                    if (match.Success)
                    {
                        var session = match.Groups[1].Value;   // Sáng, Chiều, Tối
                        var day = match.Groups[2].Value.Substring(1);   // T2, T3, T6...
                        var startPeriod = int.Parse(match.Groups[3].Value); // Tiết bắt đầu
                        var endPeriod = int.Parse(match.Groups[4].Value);   // Tiết kết thúc
                        var locale = match.Groups[5].Value;   // Địa điểm (D9-201)

                        // Nếu buổi chiều, cộng thêm 6 vào tiết bắt đầu và kết thúc
                        if (session == "Chiều")
                        {
                            startPeriod += 6;
                            endPeriod += 6;
                        }

                        // Lấy thời gian từ khung giờ
                        var startTime = timeSlotsStart[startPeriod];
                        var endTime = timeSlotsEnd[endPeriod];

                        // Thêm vào danh sách kết quả
                        result.Add(new TimeTableResponse
                        {
                            CourseCode = teachingAssignment.CourseName,
                            CourseName = teachingAssignment.Name,
                            CourseId = teachingAssignment.Code,
                            Day = day,
                            TimeTable = $"{startTime} - {endTime}",
                            Locale = locale,
                            MaxEnrol = teachingAssignment.MaxEnrol,
                        });
                    }
                }
            }

            return result;
        }

        private IQueryable<Data.TeachingAssignment> BuildQuery(QueryModel queryModel, string role)
        {
            IQueryable<Data.TeachingAssignment> query;

            // Kiểm tra role
            if (role == "lanhdao" || role == "admin")
            {
                query = _context.TeachingAssignments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải lanhdao hoặc admin
                query = _context.TeachingAssignments.Where(p => p.TeacherCode == role);
            }

            var predicate = PredicateBuilder.New<Data.TeachingAssignment>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.Name.ToLower().Contains(ts.ToLower()) ||
                        p.Code.ToLower().Contains(ts.ToLower()) ||
                        p.TeacherCode.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }
    }
}
