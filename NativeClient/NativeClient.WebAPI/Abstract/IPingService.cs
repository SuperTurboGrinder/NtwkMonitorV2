using System.Threading.Tasks;
using System.Net;

using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Abstract {

public interface IPingService {
    Task<PingTestData> TestConnectionAsync(IPAddress ip);
}

}