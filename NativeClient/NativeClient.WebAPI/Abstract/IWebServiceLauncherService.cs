using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Abstract
{
    public interface IWebServiceLauncherService
    {
        StatusMessage Start(string uri);
    }
}