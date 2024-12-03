using LinqKit;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TeachingAssignmentApp.Business.Aspiration;
using TeachingAssignmentApp.Business.Class;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProjectAssigment
{
    public class ProjectAssignmentRepository : IProjectAssignmentRepository
    {
        private readonly TeachingAssignmentDbContext _context;
        private readonly IAspirationRepository _aspirationRepository;

        public ProjectAssignmentRepository(TeachingAssignmentDbContext context, IAspirationRepository aspirationRepository)
        {
            _context = context;
            _aspirationRepository = aspirationRepository;
        }

        public async Task<Pagination<Data.ProjectAssigment>> GetAllAsync(QueryModel queryModel)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.ProjectAssigment> query = BuildQuery(queryModel);

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

        public async Task UpdateAsync(Data.ProjectAssigment projectAssignment)
        {
            _context.ProjectAssigments.Update(projectAssignment);
            await _context.SaveChangesAsync();
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

        public async Task<byte[]> ExportProjectAssignment()
        {
            var projectAssignments = await _context.ProjectAssigments
                                                     .OrderBy(pa => pa.TeacherCode)
                                                     .ToListAsync();

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

                // Định dạng tiêu đề
                worksheet.Cells[1, 1, 1, 16].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 16].Style.Font.Size = 12;
                worksheet.Cells[1, 1, 1, 16].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 16].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                int row = 2;
                foreach (var group in groupedAssignments)
                {
                    // Thêm thông tin giảng viên
                    worksheet.Cells[row, 1].Value = group.TeacherCode;
                    worksheet.Cells[row, 2].Value = group.TeacherName;
                    worksheet.Cells[row, 15].Value = group.TotalGdInstruct;

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

                        row++; // Di chuyển xuống dòng tiếp theo
                    }
                }

                // Tự động căn chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Trả về file Excel dưới dạng byte array
                return package.GetAsByteArray();
            }
        }


        private IQueryable<Data.ProjectAssigment> BuildQuery(QueryModel queryModel)
        {
            IQueryable<Data.ProjectAssigment> query = _context.ProjectAssigments;

            var predicate = PredicateBuilder.New<Data.ProjectAssigment>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
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
