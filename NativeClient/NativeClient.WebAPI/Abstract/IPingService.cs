using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Data.Model.ResultsModel;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Abstract
{
    public interface IPingService
    {
        Task<DataActionResult<PingTestData>> TestConnectionAsync(IPAddress ip);

        Task<DataActionResult<IEnumerable<PingTestData>>> TestConnectionListAsync(
            IEnumerable<IPAddress> ips
        );
    }
}