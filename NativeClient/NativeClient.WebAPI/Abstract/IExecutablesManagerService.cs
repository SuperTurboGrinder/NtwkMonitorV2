using System.Net;

using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Abstract {

//SINGLETON SERVICE
interface IExecutablesManagerService {
    string ExecuteService(ExecutableServicesTypes serviceType, IPAddress address);
}

}