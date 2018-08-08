using System.Threading.Tasks;
using System.Net;

using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Abstract {

public interface IWebServiceLauncherService {
    string Start(string uri);
}

}