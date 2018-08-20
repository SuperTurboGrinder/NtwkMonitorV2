using System.Net;

using Data.Model.ResultsModel;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Abstract {

//SINGLETON SERVICE
public interface IErrorReportAssemblerService {
    ErrorReport AssembleReport(StatusMessage status);
}

}