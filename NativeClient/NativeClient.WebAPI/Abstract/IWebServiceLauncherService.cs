using System.Threading.Tasks;
using System.Net;

using NativeClient.WebAPI.Services.Model;
using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Abstract {

public interface IWebServiceLauncherService {
    StatusMessage Start(string uri);
}

}