using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Feedback
{
    public interface IFeedbackReposiotry
    {
        Task<Data.Feedback> CreateAsync(Data.Feedback feedback);
        Task<Pagination<Data.Feedback>> GetAllAsync(QueryModel queryModel, string role);
        Task<Data.Feedback> GetByIdAsync(Guid id);
        Task<Data.Feedback> UpdateAsync(Guid id, Data.Feedback updatedFeedback);
        Task<bool> DeleteAsync(Guid id);
    }
}
