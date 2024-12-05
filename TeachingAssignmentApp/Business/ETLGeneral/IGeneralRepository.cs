using System.Threading.Tasks;
using TeachingAssignmentApp.Business.ETLGeneral.Model;

namespace TeachingAssignmentApp.Business.ETLGeneral
{
    public interface IGeneralRepository
    {
        Task<IEnumerable<ETLGenteralResponse>> GetAllAync(ETLGeneralQueryModel model);
        Task<IEnumerable<Data.Teacher>> ListAllTeacherAsync();
        Task<IEnumerable<Data.ProfessionalGroup>> ListAllProfessionalGroupAsync();
        Task<IEnumerable<Data.Class>> ListAllClassAsync();
        Task<IEnumerable<Data.Aspiration>> ListAllAspirationAsync();
        Task DeleteByTypeAsync(List<string> types);
        Task<IEnumerable<Data.ETLGeneral>> SaveAsync(IEnumerable<Data.ETLGeneral> generals);
    }
}
