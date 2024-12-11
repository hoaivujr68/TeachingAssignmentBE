using LinqKit;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Business.Teacher;
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

        public async Task<Pagination<AspirationModel>> GetProjectNotAssignmentAsync(QueryModel queryModel)
        {
            var queryProjectModel = new QueryModel
            {
                CurrentPage = 1,
                PageSize = 300,
                ListTextSearch = queryModel.ListTextSearch
            };

            var allProjects = await _aspirationRepository.GetAllAsync(queryProjectModel);

            var assignedProjectCodes = await _context.ProjectAssigments
                                             .Select(t => t.StudentId)
                                             .ToListAsync();

            var aspirationesNotAssigned = allProjects.Content
                                                .Where(c => !assignedProjectCodes.Contains(c.StudentId))
                                                .ToList();

            var result = new Pagination<AspirationModel>(
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

            // Lấy dữ liệu từ Aspiration và Project trước, rồi xử lý nhóm dữ liệu
            var gdData = await (from a in _context.Aspirations
                                join p in _context.Projects on a.ClassName equals p.CourseName
                                select new { p.CourseName, p.GdInstruct })
                                 .ToListAsync();  // Lấy dữ liệu về bộ nhớ

            // Nhóm và tạo Dictionary với khóa là CourseName và giá trị là GdInstruct
            var gdDictionary = gdData
                                .GroupBy(x => x.CourseName)
                                .ToDictionary(g => g.Key, g => g.FirstOrDefault().GdInstruct ?? 0.0);

            // Tính tổng GdInstruct từ Aspiration sau khi so sánh với gdDictionary
            double totalGd = 0.0;

            foreach (var aspiration in _context.Aspirations)
            {
                if (gdDictionary.ContainsKey(aspiration.ClassName))
                {
                    totalGd += gdDictionary[aspiration.ClassName];
                }
            }

            // Kiểm tra tránh chia cho 0
            if (totalGdInstruct == 0) return null;

            // Trả về kết quả sau khi chia và làm tròn
            return Math.Round(totalGd / totalGdInstruct, 2);
        }

        public async Task<IEnumerable<TeacherModel>> GetAvailableTeachersForStudentId(string studentId)
        {
            // 1. Lấy thông tin lớp học theo mã lớp
            var aspirationEntity = await _context.Aspirations
                .FirstOrDefaultAsync(c => c.StudentId == studentId);

            var projectAssignment = await _context.ProjectAssigments
                .FirstOrDefaultAsync(pa => pa.StudentId == studentId);

            if (aspirationEntity == null)
            {
                throw new ArgumentException($"Class with ID {studentId} not found.");
            }

            // 2. Lấy phân công hiện tại cho lớp học
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
                var teacherAssignments = await _context.ProjectAssigments
                    .Where(pa => pa.TeacherCode == teacher.Code)
                    .ToListAsync();

                // Tính tổng tất cả GdInstruct của giáo viên trong ProjectAssignments
                double totalTeachingHours = teacherAssignments.Sum(pa => pa.GdInstruct ?? 0.0) + (projectAssignment?.GdInstruct ?? 0.0);

                // Kiểm tra nếu tổng giờ giảng vượt giới hạn
                if (totalTeachingHours > (teacher.GdTeaching ?? 0) * 1.4)
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
            return await _context.ProjectAssigments
                .Where(ta => ta.TeacherCode == teacherCode)
                .SumAsync(ta => ta.GdInstruct ?? 0.0);
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
            existingAssignment.Aspiration1 = updatedProjectAssigment.Aspiration1 ?? existingAssignment.Aspiration1 ;
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
                worksheet.Cells[1, 17].Value = "Tổng giờ hướng dẫn - giới hạn";
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
                    worksheet.Cells[row, 18].Value = "Thỏa mãn";

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
                worksheet.Cells[1, 17].Value = "Một nguyện vọng - Một giảng viên";
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
