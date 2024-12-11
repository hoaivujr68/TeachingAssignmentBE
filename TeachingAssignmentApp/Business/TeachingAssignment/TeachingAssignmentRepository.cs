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
                worksheet.Cells[1, 16].Value = "Tổng giờ hướng dẫn - giới hạn";
                worksheet.Cells[1, 17].Value = "Giờ giảng dạy cân bằng";

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 17].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 17].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                int row = 2;
                foreach (var group in groupedAssignments)
                {
                    // Thêm thông tin giảng viên
                    worksheet.Cells[row, 1].Value = group.TeacherCode;
                    worksheet.Cells[row, 12].Value = group.TotalGdTeaching;

                    // Tìm GdTeaching của giảng viên trong dictionary
                    if (teacherDictionary.TryGetValue(group.TeacherCode, out var teacherInfo))
                    {
                        worksheet.Cells[row, 2].Value = teacherInfo.Item1;
                        worksheet.Cells[row, 11].Value = teacherInfo.Item2;
                        worksheet.Cells[row, 13].Value = Math.Round((double)(group.TotalGdTeaching) / (double)(teacherInfo.Item2 ?? 0), 2).ToString();

                    }
                    worksheet.Cells[row, 14].Value = "Thỏa mãn";
                    worksheet.Cells[row, 15].Value = "Thỏa mãn";
                    worksheet.Cells[row, 16].Value = "Thỏa mãn";
                    worksheet.Cells[row, 17].Value = "Không thỏa mãn";

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
