using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

using NativeClient.WebAPI.Services.Model;
using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Abstract {

public interface IPingService {
    Task<DataActionResult<PingTestData>> TestConnectionAsync(IPAddress ip);
    Task<DataActionResult<IEnumerable<PingTestData>>> TestConnectionListAsync(
        IEnumerable<IPAddress> ips
    );
}

}