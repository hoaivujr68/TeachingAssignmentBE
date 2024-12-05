using TeachingAssignmentApp.Business.ETLGeneral.Model;

namespace TeachingAssignmentApp.Business.ETLGeneral
{
    public interface IGeneralService
    {
        Task<IEnumerable<ETLGenteralResponse>> GetAllAync(string type);
        Task<IEnumerable<Data.ETLGeneral>> RefreshAsync();
    }
}
