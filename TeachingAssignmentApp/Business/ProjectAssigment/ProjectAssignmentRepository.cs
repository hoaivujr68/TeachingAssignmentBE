using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO.Packaging;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Teacher;
using TeachingAssignmentApp.Business.TeachingAssignment.Model;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProjectAssigment
{
    public class ProjectAssignmentRepository : IProjectAssignmentRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        private readonly IAspirationRepository _aspirationRepository;
        private readonly ITeacherRepository _teacherRepository;
        public ProjectAssignmentRepository(TeachingAssignmentDbContext context, IAspirationRepository aspirationRepository, ITeacherRepository teacherRepository)
        {
            _context = context;
            _aspirationRepository = aspirationRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<Pagination<Data.ProjectAssigment>> GetAllAsync(QueryModel queryModel, string role)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.ProjectAssigment> query = BuildQuery(queryModel, role);
            //var test = Pagination<Data.ProjectAssigment>;
            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        public async Task<Pagination<ProjectAssignmentInput>> GetProjectNotAssignmentAsync(QueryModel queryModel)
        {
            var allProjects = await _context.ProjectAssignmentInputs.ToListAsync();

            var assignedProjectCodes = await _context.ProjectAssigments
                                             .Select(t => t.StudentId)
                                             .ToListAsync();

            var aspirationesNotAssigned = allProjects
                                                .Where(c => !assignedProjectCodes.Contains(c.StudentId))
                                                .ToList();

            var result = new Pagination<ProjectAssignmentInput>(
                aspirationesNotAssigned,
                aspirationesNotAssigned.Count,
                queryModel.CurrentPage ?? 1,
                queryModel.PageSize ?? 20
            );

            return result;
        }

        public async Task<Pagination<TeacherModel>> GetTeacherNotAssignmentAsync(QueryModel queryModel)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 100,
                ListTextSearch = queryModel.ListTextSearch
            };

            var allTeachers = await _teacherRepository.GetAllAsync(queryProjectModel);

            var assignedTeacherCodes = await _context.TeachingAssignments
                                             .Select(t => t.TeacherCode)
                                             .ToListAsync();

            var aspirationesNotAssigned = allTeachers.Content
                                                .Where(c => !assignedTeacherCodes.Contains(c.Code))
                                                .ToList();

            var result = new Pagination<TeacherModel>(
                aspirationesNotAssigned,
                aspirationesNotAssigned.Count,
                queryModel.CurrentPage ?? 1,
                queryModel.PageSize ?? 20
            );

            return result;
        }

        public async Task<double?> GetRangeGdInstruct()
        {
            var totalGdInstruct = await _context.Teachers
                .SumAsync(ta => ta.GdInstruct ?? 0.0);

            var totalGd = await _context.ProjectAssignmentInputs
                .SumAsync(ta => ta.GdInstruct ?? 0.0);

            // Kiểm tra tránh chia cho 0
            if (totalGdInstruct == 0) return null;

            // Trả về kết quả sau khi chia và làm tròn
            return Math.Round(totalGd / totalGdInstruct, 2);
        }

        public async Task SwapTeacherAssignmentAsync(Guid teacherAssignmentId1, Guid teacherAssignmentId2)
        {
            var teacherAssignment1 = await _context.ProjectAssigments.FindAsync(teacherAssignmentId1);
            var teacherAssignment2 = await _context.ProjectAssigments.FindAsync(teacherAssignmentId2);

            if (teacherAssignment1 == null || teacherAssignment2 == null)
            {
                throw new InvalidOperationException("One or both teacher assignments could not be found.");
            }

            // Lưu tạm giá trị của TeacherCode từ teacherAssignment1
            var tempTeacherCode = teacherAssignment1.TeacherCode;
            var tempTeacherName = teacherAssignment1.TeacherName;

            // Hoán đổi TeacherCode giữa hai bản ghi
            teacherAssignment1.TeacherCode = teacherAssignment2.TeacherCode;
            teacherAssignment1.TeacherName = teacherAssignment2.TeacherName;
            teacherAssignment2.TeacherCode = tempTeacherCode;
            teacherAssignment2.TeacherName = tempTeacherName;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TeacherModel>> GetAvailableTeachersForStudentId(string studentId)
        {
            var projectAssignment = await _context.ProjectAssigments
                .FirstOrDefaultAsync(pa => pa.StudentId == studentId);
            IQueryable<Data.ProjectAssigment> query = _context.ProjectAssigments;
            var projectAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (projectAssignments == null || projectAssignments.Count == 0)
            {
                throw new InvalidOperationException("No projectAssignments available to export.");
            }
            var groupedAssignments = projectAssignments
            .GroupBy(pa => pa.TeacherCode)
            .Select(g => new
            {
                TeacherCode = g.Key,
                TeacherName = g.First().TeacherName,
                TotalGdInstruct = g.Sum(pa => pa.GdInstruct),
                Assignments = g.ToList()
            })
            .OrderBy(g => g.TeacherCode)
            .ToList();

            var currentAssignment = await _context.ProjectAssigments
                .FirstOrDefaultAsync(ta => ta.StudentId == studentId);

            string currentTeacherCode = currentAssignment?.TeacherCode;
            var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Code == currentTeacherCode);

            // 3. Lấy danh sách giáo viên dạy môn học tương ứng
            var teachers = await _context.Teachers.ToListAsync();

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

                var test = groupedAssignments
                    .FirstOrDefault(g => g.TeacherCode == teacher.Code);

                if (test.Assignments.Count > 30)
                {
                    continue;
                }

                var teacherAssignments = await _context.ProjectAssigments
                    .Where(pa => pa.TeacherCode == teacher.Code)
                    .ToListAsync();


                // Tính tổng tất cả GdInstruct của giáo viên trong ProjectAssignments
                double totalTeachingHours = teacherAssignments.Sum(pa => pa.GdInstruct ?? 0.0) + (projectAssignment?.GdInstruct ?? 0.0);

                // Kiểm tra nếu tổng giờ giảng vượt giới hạn
                if (totalTeachingHours > (teacher.GdInstruct ?? 0) * 1.5)
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

            return availableTeachers;
        }

        public async Task<double> GetTotalGdTeachingByTeacherCode(string teacherCode)
        {
            var test = await _context.ProjectAssigments
                .Where(ta => ta.TeacherCode == teacherCode)
                .SumAsync(ta => ta.GdInstruct ?? 0.0);

            return Math.Round(test, 2);
        }

        public async Task<Data.ProjectAssigment> GetByIdAsync(Guid id)
        {
            return await _context.ProjectAssigments.FindAsync(id);
        }

        public async Task AddAsync(Data.ProjectAssigment projectAssignment)
        {
            if (projectAssignment.Id == Guid.Empty)
            {
                projectAssignment.Id = Guid.NewGuid();
            }

            await _context.ProjectAssigments.AddAsync(projectAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ProjectAssignmentInput> projectAssignments)
        {
            await _context.ProjectAssignmentInputs.AddRangeAsync(projectAssignments);
            await _context.SaveChangesAsync();
        }

        public async Task<Data.ProjectAssignmentInput> GetByStudentIdAsync(string studentId)
        {
            return await _context.ProjectAssignmentInputs.FirstOrDefaultAsync(c => c.StudentId == studentId);
        }

        public async Task<Data.ProjectAssigment> UpdateAsync(Guid id, Data.ProjectAssigment updatedProjectAssigment)
        {
            var existingAssignment = await _context.ProjectAssigments
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingAssignment == null)
            {
                throw new KeyNotFoundException($"ProjectAssigments với Id '{id}' không tồn tại.");
            }

            var teacher = await _teacherRepository.GetByCodeAsync(updatedProjectAssigment.TeacherCode);
            if (teacher == null)
            {
                throw new KeyNotFoundException($"Teacher với Code '{updatedProjectAssigment.TeacherCode}' không tồn tại.");
            }

            existingAssignment.TeacherCode = updatedProjectAssigment.TeacherCode ?? existingAssignment.TeacherCode;
            existingAssignment.TeacherName = teacher.Name;
            existingAssignment.StudentId = updatedProjectAssigment.StudentId ?? existingAssignment.StudentId;
            existingAssignment.StudentName = updatedProjectAssigment.StudentName ?? existingAssignment.StudentName;
            existingAssignment.Topic = updatedProjectAssigment.Topic ?? existingAssignment.Topic;
            existingAssignment.ClassName = updatedProjectAssigment.ClassName ?? existingAssignment.ClassName;
            existingAssignment.GroupName = updatedProjectAssigment.GroupName ?? existingAssignment.GroupName;
            existingAssignment.Status = updatedProjectAssigment.Status ?? existingAssignment.Status;
            existingAssignment.DesireAccept = updatedProjectAssigment.DesireAccept ?? existingAssignment.DesireAccept;
            existingAssignment.Aspiration1 = updatedProjectAssigment.Aspiration1 ?? existingAssignment.Aspiration1;
            existingAssignment.Aspiration2 = updatedProjectAssigment.Aspiration2 ?? existingAssignment.Aspiration2;
            existingAssignment.Aspiration3 = updatedProjectAssigment.Aspiration3 ?? existingAssignment.Aspiration3;
            existingAssignment.GdInstruct = updatedProjectAssigment.GdInstruct ?? existingAssignment.GdInstruct;
            existingAssignment.StatusCode = updatedProjectAssigment.StatusCode ?? existingAssignment.StatusCode;

            _context.ProjectAssigments.Update(existingAssignment);
            await _context.SaveChangesAsync();
            return existingAssignment;
        }

        public async Task DeleteAsync(Guid id)
        {
            var projectAssignment = await _context.ProjectAssigments.FindAsync(id);
            if (projectAssignment != null)
            {
                _context.ProjectAssigments.Remove(projectAssignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<Data.ProjectAssigment> projectAssignments)
        {
            await _context.ProjectAssigments.AddRangeAsync(projectAssignments);
            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> ExportProjectAssignmentByQuota()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thống kê phân công");
                var projectAssignments = await _context.ProjectAssigments.ToListAsync();
                worksheet.Cells[1, 5].Value = "Chưa phân công";
                worksheet.Cells[2, 5].Value = "Tổng";
                worksheet.Cells[3, 5].Value = "Đã phân công + Nguyện vọng";
                worksheet.Cells[4, 5].Value = "GD loại đồ án";
                worksheet.Cells[5, 5].Value = "Quota max (1.5 trung bình)";
                worksheet.Cells[6, 5].Value = "Loại đồ án";
                worksheet.Cells[1, 6].Value = 0;
                worksheet.Cells[2, 6].Value = projectAssignments.Count;
                worksheet.Cells[3, 6].Value = projectAssignments.Count;
                worksheet.Cells[7, 1].Value = "STT";
                worksheet.Cells[7, 2].Value = "Giảng viên";
                worksheet.Cells[7, 3].Value = "GD được phân";
                worksheet.Cells[7, 4].Value = "Tổng phân công";

                var groupedAssignments = projectAssignments
                    .GroupBy(p => new { p.ClassName, p.GdInstruct })
                    .Select(group => new
                    {
                        ClassName = group.Key.ClassName,
                        GdInstruct = group.Key.GdInstruct,
                        NameAndGroups = string.Join(", ", group
                            .GroupBy(p => p.Name)
                            .Select(subGroup => $"{string.Join(", ", subGroup.Select(p => $"{p.ClassName} {p.Name} {p.GroupName}").Distinct())}") // Gắn ClassName vào GroupName
                        ),
                        TotalCount = group.Count(), // Tổng số bản ghi trong nhóm
                        QuotaRange = Math.Ceiling(group.Count() / 49.0 * 1.5)
            })
                    .OrderBy(g => g.GdInstruct) // Sắp xếp theo GdInstruct
                    .ThenBy(g => g.ClassName)   // Sau đó sắp xếp theo ClassName
                    .ToList();

                var groupedAssignmentTeachers = projectAssignments
                    .GroupBy(p => new { p.TeacherCode, p.TeacherName })
                    .Select(teacherGroup => new
                    {
                        TeacherCode = teacherGroup.Key.TeacherCode,
                        TeacherName = teacherGroup.Key.TeacherName,
                        GdInstruct = teacherGroup.Sum(t => t.GdInstruct), // Tổng GdInstruct của tất cả bản ghi của giảng viên
                        Assignments = teacherGroup
                            .GroupBy(p => new { p.ClassName, p.GdInstruct })
                            .Select(classGroup => new
                            {
                                ClassName = classGroup.Key.ClassName,
                                GdInstruct = classGroup.Key.GdInstruct,
                                TotalCount = classGroup.Count(),
                                NameAndGroups = string.Join(", ", classGroup
                                    .Select(p => $"{p.ClassName} {p.Name} {p.GroupName}")
                                    .Distinct())
                            })
                            .ToList()
                    })
                    .ToList();

                var groupedAssignmentGroups = projectAssignments
                   .GroupBy(p => new { p.GroupName, p.GdInstruct })
                   .Select(group => new
                   {
                       GroupName = group.Key.GroupName,
                       GdInstruct = group.Key.GdInstruct,
                       NameAndGroups = string.Join(", ", group
                           .GroupBy(p => p.Name)
                           .Select(subGroup => $"{string.Join(", ", subGroup.Select(p => $"{p.GroupName} {p.Name}").Distinct())}") // Gắn ClassName vào GroupName
                       ),
                       TotalCount = group.Count(), // Tổng số bản ghi trong nhóm
                       QuotaRange = Math.Ceiling(group.Count() / 49.0 * 1.5)
                   })
                   .OrderBy(g => g.GdInstruct) // Sắp xếp theo GdInstruct
                   .ThenBy(g => g.GroupName)   // Sau đó sắp xếp theo ClassName
                   .ToList();

                var groupedAssignmentTeacherGroups = projectAssignments
                    .GroupBy(p => new { p.TeacherCode, p.TeacherName })
                    .Select(teacherGroup => new
                    {
                        TeacherCode = teacherGroup.Key.TeacherCode,
                        TeacherName = teacherGroup.Key.TeacherName,
                        GdInstruct = teacherGroup.Sum(t => t.GdInstruct), // Tổng GdInstruct của tất cả bản ghi của giảng viên
                        Assignments = teacherGroup
                            .GroupBy(p => new { p.GroupName, p.GdInstruct })
                            .Select(classGroup => new
                            {
                                GroupName = classGroup.Key.GroupName,
                                GdInstruct = classGroup.Key.GdInstruct,
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
                    worksheet.Cells[i + 8, 2].Value = teacherGroup.TeacherName; // Tên giảng viên
                    worksheet.Cells[i + 8, 3].Value = Math.Round(teacherGroup.GdInstruct ?? 0.0, 2); // Tổng GdInstruct
                    worksheet.Cells[i + 8, 4].Value = teacherGroup.Assignments.Sum(p => p.TotalCount); // Tổng GdInstruct

                    for (int j = 0; j < groupedAssignments.Count; j++)
                    {
                        var assignment = groupedAssignments[j];

                        // Kiểm tra nếu ClassName và GdInstruct của giảng viên phù hợp với nhóm trong groupedAssignments
                        var matchedAssignment = teacherGroup.Assignments
                            .FirstOrDefault(a => a.ClassName == assignment.ClassName && a.GdInstruct == assignment.GdInstruct);

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
                    worksheet.Cells[4, i + 7].Value = groupedAssignments[i].GdInstruct;
                    worksheet.Cells[5, i + 7].Value = groupedAssignments[i].QuotaRange;
                    worksheet.Cells[6, i + 7].Value = groupedAssignments[i].NameAndGroups;
                }
                var groupedAssignmentsCount = groupedAssignments.Count;
                worksheet.Cells[4, 8 + groupedAssignmentsCount].Value = "Thống kê theo hệ đào tạo";
                worksheet.Cells[1, 9 + groupedAssignmentsCount].Value = 0;
                worksheet.Cells[2, 9 + groupedAssignmentsCount].Value = projectAssignments.Count;
                worksheet.Cells[3, 9 + groupedAssignmentsCount].Value = projectAssignments.Count;
                for (int i = 0; i < groupedAssignmentGroups.Count; i++)
                {
                    worksheet.Cells[1, i + 10 + groupedAssignmentsCount].Value = 0;
                    worksheet.Cells[2, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].TotalCount;
                    worksheet.Cells[3, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].TotalCount;
                    worksheet.Cells[4, i + 10 + groupedAssignmentsCount].Value = groupedAssignmentGroups[i].GdInstruct;
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
                            .FirstOrDefault(a => a.GroupName == assignment.GroupName && a.GdInstruct == assignment.GdInstruct);

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

        public async Task<byte[]> ExportProjectAssignment(string role)
        {
            IQueryable<Data.ProjectAssigment> query;

            if (role == "lanhdao" || role == "admin")
            {
                query = _context.ProjectAssigments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải lanhdao hoặc admin
                query = _context.ProjectAssigments.Where(pa => pa.TeacherCode == role);
            }

            // Sắp xếp và lấy dữ liệu
            var projectAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (projectAssignments == null || projectAssignments.Count == 0)
            {
                throw new InvalidOperationException("No projectAssignments available to export.");
            }

            var teachers = await _context.Teachers.ToListAsync();

            // Nhóm phân công theo TeacherCode và tính tổng GdInstruct của từng giảng viên
            var groupedAssignments = projectAssignments
                .GroupBy(pa => pa.TeacherCode)
                .Select(g => new
                {
                    TeacherCode = g.Key,
                    TeacherName = g.First().TeacherName,
                    TotalGdInstruct = g.Sum(pa => pa.GdInstruct),
                    Assignments = g.ToList()
                })
                .OrderBy(g => g.TeacherCode)
                .ToList();

            // Tạo dictionary để tra cứu GdInstruct của giảng viên nhanh hơn
            var teacherDictionary = teachers.ToDictionary(t => t.Code, t => t.GdInstruct);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phân công hướng dẫn đồ án");

                // Thêm tiêu đề cột
                worksheet.Cells[1, 1].Value = "Mã giảng viên";
                worksheet.Cells[1, 2].Value = "Tên giảng viên";
                worksheet.Cells[1, 3].Value = "Mã sinh viên";
                worksheet.Cells[1, 4].Value = "Tên sinh viên";
                worksheet.Cells[1, 5].Value = "Đề tài";
                worksheet.Cells[1, 6].Value = "Mã đồ án";
                worksheet.Cells[1, 7].Value = "Hệ";
                worksheet.Cells[1, 8].Value = "Trạng thái";
                worksheet.Cells[1, 9].Value = "Nguyện vọng xác nhận";
                worksheet.Cells[1, 10].Value = "Nguyện vọng 1";
                worksheet.Cells[1, 11].Value = "Nguyện vọng 2";
                worksheet.Cells[1, 12].Value = "Nguyện vọng 3";
                worksheet.Cells[1, 13].Value = "Gd đồ án";
                worksheet.Cells[1, 14].Value = "Gd giảng viên";
                worksheet.Cells[1, 15].Value = "Tổng Gd phân công theo giảng viên";
                worksheet.Cells[1, 16].Value = "Tỷ lệ";
                worksheet.Cells[1, 17].Value = "Số giờ phân công thỏa mãn";
                worksheet.Cells[1, 18].Value = "Hướng dẫn tối đa 30 sinh viên/kỳ";
                worksheet.Cells[1, 19].Value = "Nguyện vọng hướng dẫn đúng";

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 19].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 19].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 19].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                int row = 2;
                foreach (var group in groupedAssignments)
                {
                    // Thêm thông tin giảng viên
                    worksheet.Cells[row, 1].Value = group.TeacherCode;
                    worksheet.Cells[row, 2].Value = group.TeacherName;
                    worksheet.Cells[row, 15].Value = group.TotalGdInstruct;
                    worksheet.Cells[row, 17].Value = "Thỏa mãn";
                    if (group.Assignments.Count > 30)
                    {
                        worksheet.Cells[row, 18].Value = "Không thỏa mãn";
                    }
                    else
                    {
                        worksheet.Cells[row, 18].Value = "Thỏa mãn";
                    }

                    // Tìm GdInstruct của giảng viên trong dictionary
                    if (teacherDictionary.TryGetValue(group.TeacherCode, out var teacherGdInstruct))
                    {
                        worksheet.Cells[row, 14].Value = teacherGdInstruct;
                        worksheet.Cells[row, 16].Value = Math.Round((double)(group.TotalGdInstruct ?? 0) / (double)(teacherGdInstruct ?? 0), 2).ToString();
                    }

                    // Thêm dữ liệu phân công của từng giảng viên
                    foreach (var assignment in group.Assignments)
                    {
                        worksheet.Cells[row, 3].Value = assignment.StudentId;
                        worksheet.Cells[row, 4].Value = assignment.StudentName;
                        worksheet.Cells[row, 5].Value = assignment.Topic;
                        worksheet.Cells[row, 6].Value = assignment.ClassName;
                        worksheet.Cells[row, 7].Value = assignment.GroupName;
                        worksheet.Cells[row, 8].Value = assignment.Status;
                        worksheet.Cells[row, 9].Value = assignment.DesireAccept;
                        worksheet.Cells[row, 10].Value = assignment.Aspiration1;
                        worksheet.Cells[row, 11].Value = assignment.Aspiration2;
                        worksheet.Cells[row, 12].Value = assignment.Aspiration3;
                        worksheet.Cells[row, 13].Value = assignment.GdInstruct;
                        if (group.TeacherName == assignment.Aspiration1 || group.TeacherName == assignment.Aspiration2 || group.TeacherName == assignment.Aspiration3)
                        {
                            worksheet.Cells[row, 19].Value = "Thỏa mãn";
                        }
                        else
                        {
                            worksheet.Cells[row, 19].Value = "Không thỏa mãn";
                        }
                        row++; // Di chuyển xuống dòng tiếp theo
                    }
                }

                // Tự động căn chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Trả về file Excel dưới dạng byte array
                return package.GetAsByteArray();
            }
        }

        public async Task<IEnumerable<TeacherResultError>> GetMaxAsync()
        {
            IQueryable<Data.ProjectAssigment> query = _context.ProjectAssigments;
            var projectAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (projectAssignments == null || projectAssignments.Count == 0)
            {
                throw new InvalidOperationException("No projectAssignments available to export.");
            }

            var groupedAssignments = projectAssignments
              .GroupBy(pa => pa.TeacherCode)
              .Select(g => new
              {
                  TeacherCode = g.Key,
                  TeacherName = g.First().TeacherName,
                  TotalGdInstruct = g.Sum(pa => pa.GdInstruct),
                  Assignments = g.ToList()
              })
              .OrderBy(g => g.TeacherCode)
              .ToList();

            var teacherResult = new List<TeacherResultError>();

            var teacherCountRB6 = 0;
            foreach (var group in groupedAssignments)
            {
                if (group.Assignments.Count > 30)
                {
                    var countError = group.Assignments.Count; // Gán giá trị để sử dụng nhiều lần

                    teacherResult.Add(new TeacherResultError
                    {
                        Code = group.TeacherCode,
                        Name = group.TeacherName,
                        CountError = countError,
                        Message = $"Được phân công hướng dẫn {countError} sinh viên"
                    });

                    teacherCountRB6 += 1;
                }
            }
            return teacherResult;

        }

        public async Task<IEnumerable<ResultModel>> GetResultAsync()
        {
            var teachers = await _context.Teachers.ToListAsync();
            var aspirations = await _context.ProjectAssignmentInputs.ToListAsync();
            var totalGdInstruct = await _context.Teachers
                .SumAsync(ta => ta.GdInstruct ?? 0.0);
            var aspirations1 = await _context.Aspirations.ToListAsync();
            // Tính tổng GdInstruct từ Aspiration sau khi so sánh với gdDictionary
            double totalGd = 0.0;
            var test = await _context.ProjectAssignmentInputs
                .SumAsync(ta => ta.GdInstruct ?? 0.0);

            totalGd += test;
            totalGdInstruct = Math.Round(totalGdInstruct, 2);
            totalGd = Math.Round(totalGd, 2);
            var rateGd = Math.Round(totalGd / totalGdInstruct, 2);
            var teacherCountRB9 = 0;
            IQueryable<Data.ProjectAssigment> query = _context.ProjectAssigments;
            var projectAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (projectAssignments == null || projectAssignments.Count == 0)
            {
                throw new InvalidOperationException("No projectAssignments available to export.");
            }

            // Nhóm phân công theo TeacherCode và tính tổng GdInstruct của từng giảng viên
            var groupedAssignments = projectAssignments
                .GroupBy(pa => pa.TeacherCode)
                .Select(g => new
                {
                    TeacherCode = g.Key,
                    TeacherName = g.First().TeacherName,
                    TotalGdInstruct = g.Sum(pa => pa.GdInstruct),
                    Assignments = g.ToList()
                })
                .OrderBy(g => g.TeacherCode)
                .ToList();

            var teacherRB6 = 0;
            foreach (var group in groupedAssignments)
            {
                if (group.Assignments.Count > 30)
                {
                    teacherRB6 += 1;
                }
            }

            teacherCountRB9 += groupedAssignments
                .Sum(group => group.Assignments
                    .Count(assignment =>
                        (assignment.Aspiration1 != null && group.TeacherName == assignment.Aspiration1) ||
                        (assignment.Aspiration2 != null && group.TeacherName == assignment.Aspiration2) ||
                        (assignment.Aspiration3 != null && group.TeacherName == assignment.Aspiration3)));

            var result = new List<ResultModel>
            {
                new ResultModel
                {
                    Label = "Một đồ án - Một giảng viên",
                    Value = aspirations.Count,
                    Code = "RB4",
                    Type = "Table",
                    Category = "đồ án"
                },
                new ResultModel
                {
                    Label = "Số giờ phân công thỏa mãn",
                    Value = teachers.Count,
                    Code = "RB5",
                    Type = "Table",
                    Category = "giảng viên"
                },
                new ResultModel
                {
                    Label = "Hướng dẫn tối đa 30 sinh viên/kỳ",
                    Value = teacherRB6,
                    Code = "RB6",
                    Type = "Table",
                    Category = "giảng viên"
                },
                new ResultModel
                {
                    Label = "Nguyện vọng hướng dẫn đúng",
                    Value = teacherCountRB9,
                    Code = "RB9",
                    Type = "Table",
                    Category = "nguyện vọng"
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
                    Label = "Số đồ án",
                    Value = aspirations.Count,
                    Code = "NV",
                    Type = "Header",
                    Category = "đồ án"
                },
                new ResultModel
                {
                    Label = "Tổng GD giảng viên",
                    Value = totalGdInstruct,
                    Code = "totalGdInstruct",
                    Type = "Header",
                    Category = "giảng viên"
                }
                   ,
                   new ResultModel
                {
                    Label = "Tổng GD đồ án",
                    Value = totalGd,
                    Code = "totalGd",
                    Type = "Header",
                    Category = "giảng viên"
                }
                   ,
                   new ResultModel
                {
                    Label = "Tỷ lệ trung bình",
                    Value = rateGd,
                    Code = "rateGd",
                    Type = "Header",
                    Category = "giảng viên"
                },
                   new ResultModel
                {
                    Label = "Số nguyện vọng",
                    Value = aspirations1.Count,
                    Code = "NV1",
                    Type = "Header",
                    Category = "giảng viên"
                }
            };

            return result;
        }

        public async Task<byte[]> ExportAspirationAssignment(string role)
        {
            IQueryable<Data.ProjectAssigment> query;

            if (role == "lanhdao" || role == "admin")
            {
                query = _context.ProjectAssigments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải lanhdao hoặc admin
                query = _context.ProjectAssigments.Where(pa => pa.TeacherCode == role);
            }

            // Sắp xếp và lấy dữ liệu
            var projectAssignments = await query.OrderBy(pa => pa.TeacherCode).ToListAsync();

            if (projectAssignments == null || projectAssignments.Count == 0)
            {
                throw new InvalidOperationException("No projectAssignments available to export.");
            }

            var teachers = await _context.Teachers.ToListAsync();

            // Nhóm phân công theo TeacherCode và tính tổng GdInstruct của từng giảng viên
            var groupedAssignments = projectAssignments
                .GroupBy(pa => pa.TeacherCode)
                .Select(g => new
                {
                    TeacherCode = g.Key,
                    TeacherName = g.First().TeacherName,
                    TotalGdInstruct = g.Sum(pa => pa.GdInstruct),
                    Assignments = g.ToList()
                })
                .OrderBy(g => g.TeacherCode)
                .ToList();

            // Tạo dictionary để tra cứu GdInstruct của giảng viên nhanh hơn
            var teacherDictionary = teachers.ToDictionary(t => t.Code, t => t.GdInstruct);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phân công hướng dẫn đồ án");

                // Thêm tiêu đề cột
                worksheet.Cells[1, 1].Value = "Mã sinh viên";
                worksheet.Cells[1, 2].Value = "Tên sinh viên";
                worksheet.Cells[1, 3].Value = "Mã đồ án";
                worksheet.Cells[1, 4].Value = "Đề tài";
                worksheet.Cells[1, 5].Value = "Hệ";
                worksheet.Cells[1, 6].Value = "Trạng thái";
                worksheet.Cells[1, 7].Value = "Nguyện vọng xác nhận";
                worksheet.Cells[1, 8].Value = "Nguyện vọng 1";
                worksheet.Cells[1, 9].Value = "Nguyện vọng 2";
                worksheet.Cells[1, 10].Value = "Nguyện vọng 3";
                worksheet.Cells[1, 11].Value = "Gd đồ án";
                worksheet.Cells[1, 12].Value = "Mã giảng viên";
                worksheet.Cells[1, 13].Value = "Tên giảng viên";
                worksheet.Cells[1, 14].Value = "Gd giảng viên";
                worksheet.Cells[1, 15].Value = "Tổng Gd phân công theo giảng viên";
                worksheet.Cells[1, 16].Value = "Tỷ lệ";
                worksheet.Cells[1, 17].Value = "Một đồ án - Một giảng viên";
                worksheet.Cells[1, 18].Value = "Nguyện vọng hướng dẫn đúng";

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 18].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 18].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 18].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                int row = 2;
                foreach (var group in groupedAssignments)
                {

                    // Thêm dữ liệu phân công của từng giảng viên
                    foreach (var assignment in group.Assignments)
                    {
                        // Thêm thông tin giảng viên
                        worksheet.Cells[row, 12].Value = group.TeacherCode;
                        worksheet.Cells[row, 13].Value = group.TeacherName;
                        worksheet.Cells[row, 15].Value = group.TotalGdInstruct;
                        worksheet.Cells[row, 17].Value = "Thỏa mãn";

                        // Tìm GdInstruct của giảng viên trong dictionary
                        if (teacherDictionary.TryGetValue(group.TeacherCode, out var teacherGdInstruct))
                        {
                            worksheet.Cells[row, 14].Value = teacherGdInstruct;
                            worksheet.Cells[row, 16].Value = Math.Round((double)(group.TotalGdInstruct ?? 0) / (double)(teacherGdInstruct ?? 0), 2).ToString();
                        }
                        worksheet.Cells[row, 1].Value = assignment.StudentId;
                        worksheet.Cells[row, 2].Value = assignment.StudentName;
                        worksheet.Cells[row, 3].Value = assignment.Topic;
                        worksheet.Cells[row, 4].Value = assignment.ClassName;
                        worksheet.Cells[row, 5].Value = assignment.GroupName;
                        worksheet.Cells[row, 6].Value = assignment.Status;
                        worksheet.Cells[row, 7].Value = assignment.DesireAccept;
                        worksheet.Cells[row, 8].Value = assignment.Aspiration1;
                        worksheet.Cells[row, 9].Value = assignment.Aspiration2;
                        worksheet.Cells[row, 10].Value = assignment.Aspiration3;
                        worksheet.Cells[row, 11].Value = assignment.GdInstruct;
                        if (group.TeacherName == assignment.Aspiration1 || group.TeacherName == assignment.Aspiration2 || group.TeacherName == assignment.Aspiration3)
                        {
                            worksheet.Cells[row, 18].Value = "Thỏa mãn";
                        }
                        else
                        {
                            worksheet.Cells[row, 18].Value = "Không thỏa mãn";
                        }
                        row++; // Di chuyển xuống dòng tiếp theo
                    }
                }

                // Tự động căn chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Trả về file Excel dưới dạng byte array
                return package.GetAsByteArray();
            }
        }

        private IQueryable<Data.ProjectAssigment> BuildQuery(QueryModel queryModel, string role)
        {
            IQueryable<Data.ProjectAssigment> query;

            // Kiểm tra role
            if (role == "lanhdao" || role == "admin")
            {
                query = _context.ProjectAssigments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải lanhdao hoặc admin
                query = _context.ProjectAssigments.Where(p => p.TeacherCode == role);
            }

            // Xây dựng predicate cho ListTextSearch
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                var predicate = PredicateBuilder.New<Data.ProjectAssigment>();
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.TeacherCode.ToLower().Contains(ts.ToLower()) ||
                        p.TeacherName.ToLower().Contains(ts.ToLower()) ||
                        p.StudentId.ToLower().Contains(ts.ToLower()) ||
                        p.StudentName.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }

    }
}
