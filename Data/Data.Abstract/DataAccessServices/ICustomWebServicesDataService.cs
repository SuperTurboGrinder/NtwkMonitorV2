using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices
{
//validation -> conversion -> DataActionResult
    public interface ICustomWebServicesDataService
    {
        Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCws();

        Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

        Task<StatusMessage> CreateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3);

        Task<StatusMessage> UpdateCustomWebService(CustomWebService cws);

        Task<StatusMessage> UpdateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3);

        Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsId);
        Task<StatusMessage> RemoveWebServiceBinding(int nodeId, int cwsId);
    }
}