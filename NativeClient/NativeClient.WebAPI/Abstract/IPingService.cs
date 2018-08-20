using System.Threading.Tasks;
using System.Net;

using NativeClient.WebAPI.Services.Model;
using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Abstract {

public interface IPingService {
    Task<DataActionResult<PingTestData>> TestConnectionAsync(IPAddress ip);
}

}