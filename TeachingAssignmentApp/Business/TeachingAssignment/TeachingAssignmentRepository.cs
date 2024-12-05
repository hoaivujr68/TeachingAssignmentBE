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

        public async Task UpdateAsync(Data.TeachingAssignment teachingAssignment)
        {
            _context.TeachingAssignments.Update(teachingAssignment);
            await _context.SaveChangesAsync();
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

            if (role == "Leader" || role == "admin")
            {
                query = _context.TeachingAssignments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải Leader hoặc admin
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

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 13].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 13].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

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

        public async Task AddRangeAsync(IEnumerable<Data.TeachingAssignment> teachingAssignments)
        {
            await _context.TeachingAssignments.AddRangeAsync(teachingAssignments);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Data.TeachingAssignment> BuildQuery(QueryModel queryModel, string role)
        {
            IQueryable<Data.TeachingAssignment> query;

            // Kiểm tra role
            if (role == "Leader" || role == "admin")
            {
                query = _context.TeachingAssignments;
            }
            else
            {
                // Lọc theo teacherCode khi role không phải Leader hoặc admin
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
