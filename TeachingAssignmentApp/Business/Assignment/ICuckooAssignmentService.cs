using TeachingAssignmentApp.Business.Assignment.Model;

namespace TeachingAssignmentApp.Business.Assignment
{
    public interface ICuckooAssignmentService
    {
        Task<SolutionModel> TeachingAssignmentCuckooSearch();
    }
}
