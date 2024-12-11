using LinqKit;
using Microsoft.EntityFrameworkCore;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Feedback
{
    public class FeedbackRepository : IFeedbackReposiotry
    {
        private readonly TeachingAssignmentDbContext _context;

        public FeedbackRepository(TeachingAssignmentDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<Data.Feedback> CreateAsync(Data.Feedback feedback)
        {
            if (feedback == null)
                throw new ArgumentNullException(nameof(feedback));

            // Thêm mới Feedback vào DbContext và lưu thay đổi
            feedback.Id = Guid.NewGuid();
            if (feedback.StatusCode == 1) feedback.StatusName = "Khởi tạo";
            if (feedback.StatusCode == 2) feedback.StatusName = "Phản hồi";
            if (feedback.StatusCode == 3) feedback.StatusName = "Phê duyệt";
            if (feedback.StatusCode == 4) feedback.StatusName = "Hủy bỏ";
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Code == feedback.TeacherCode);
            if (teacher == null)
                throw new KeyNotFoundException("Teacher not found.");
            feedback.TeacherName = teacher.Name;

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Pagination<Data.Feedback>> GetAllAsync(QueryModel queryModel, string role)
        {
            queryModel.PageSize ??= 20;
            queryModel.CurrentPage ??= 1;

            IQueryable<Data.Feedback> query = BuildQuery(queryModel, role);
            
            var result = await query.GetPagedOrderAsync(queryModel.CurrentPage.Value, queryModel.PageSize.Value, string.Empty);
            return result;
        }

        // READ (Get a Feedback by Id)
        public async Task<Data.Feedback> GetByIdAsync(Guid id)
        {
            return await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
        }

        // UPDATE
        public async Task<Data.Feedback> UpdateAsync(Guid id, Data.Feedback updatedFeedback)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            // Cập nhật các trường của feedback
            feedback.Code = updatedFeedback.Code;
            feedback.TeacherCode = updatedFeedback.TeacherCode;
            feedback.TeacherName = updatedFeedback.TeacherName;
            feedback.Content = updatedFeedback.Content;
            feedback.StatusCode = updatedFeedback.StatusCode;
            feedback.StatusName = updatedFeedback.StatusName;

            // Lưu lại thay đổi
            await _context.SaveChangesAsync();
            return feedback;
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid id)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Data.Feedback> BuildQuery(QueryModel queryModel,string role)
        {
            IQueryable<Data.Feedback> query;

            if (role == "lanhdao")
                query = _context.Feedbacks.AsNoTracking().Where(f => f.StatusCode == 2);
            else
                query = _context.Feedbacks.AsNoTracking();

            if (!string.IsNullOrEmpty(queryModel.Code))
            {
                query = query.Where(x => x.Code == queryModel.Code);
            }
            var predicate = PredicateBuilder.New<Data.Feedback>();
            if (queryModel.ListTextSearch != null && queryModel.ListTextSearch.Any())
            {
                foreach (var ts in queryModel.ListTextSearch)
                {
                    predicate.Or(p =>
                        p.Code.ToLower().Contains(ts.ToLower())
                    );
                }

                query = query.Where(predicate);
            }

            return query;
        }
    }

}
