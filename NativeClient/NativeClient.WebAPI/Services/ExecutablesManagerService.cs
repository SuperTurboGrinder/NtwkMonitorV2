using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using NativeClient.WebAPI.Services.Model;
using NativeClient.WebAPI.Abstract;
using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Services {

//SINGLETON SERVICE
public class ExecutablesManagerService : IExecutablesManagerService {
    class ServiceData {
        public string filePathCache;
        public string strSearchKey;
    }

    struct FilePathData {
        public string fullPath;
        public string filename;
    }

    Dictionary<ExecutableServicesTypes, ServiceData> servicesData =
        new Dictionary<ExecutableServicesTypes, ServiceData>();

    public ExecutablesManagerService() {
        lock(servicesData) {
            servicesData[ExecutableServicesTypes.SSH] = new ServiceData();
            servicesData[ExecutableServicesTypes.Telnet] = new ServiceData();
            servicesData[ExecutableServicesTypes.SSH].strSearchKey = "ssh";
            servicesData[ExecutableServicesTypes.Telnet].strSearchKey = "telnet";
        }
    }

    static DataActionResult<string> FindServicePath(
        string strKey,
        FilePathData[] paths
    ) {
        IEnumerable<FilePathData> candidates =
            paths.Where(f => f.filename.Contains(strKey));
        if(candidates.Count() == 0) {
            return DataActionResult<string>.Failed(
                StatusMessage.NoSpecifiedExecServiceFoundInBinDirectory
            );
        }
        else if(candidates.Count() > 1) {
            return DataActionResult<string>.Failed(
                StatusMessage.TooManyCandidatesForSpecefiedExecServiceInBinDirectory
            );
        }
        return DataActionResult<string>.Successful(candidates.Single().fullPath);
    }

    static DataActionResult<FilePathData[]> SearchBinDirectoryForFilePaths() {
        FilePathData[] files = null;
        try {
            files = Directory.GetFiles(Directory.GetCurrentDirectory()+@"/bin")
                .Select(f => new FilePathData() {
                    fullPath = f,
                    filename = Path.GetFileName(f)
                })
                .ToArray();
        }
        catch(UnauthorizedAccessException) {
            return DataActionResult<FilePathData[]>.Failed(
                StatusMessage.NotEnoughOSRightsToSearchForExecServices
            );
        }
        catch(DirectoryNotFoundException) {
            return DataActionResult<FilePathData[]>.Failed(
                StatusMessage.CouldNotFindBinDirectoryWhileSearchingForExecutableServices
            );
        }
        catch(IOException) {
            return DataActionResult<FilePathData[]>.Failed(
                StatusMessage.UnknownIOErrorWhileTryingToFindExecutableServices
            );
        }
        return DataActionResult<FilePathData[]>.Successful(files);
    }

    DataActionResult<string> GetFilePath(ExecutableServicesTypes key) {
        string path = null;
        lock(servicesData) {
            path = servicesData[key].filePathCache;
            if(path == null || !File.Exists(path)) {
                DataActionResult<FilePathData[]> pathsSearchResult =
                    SearchBinDirectoryForFilePaths();
                if(pathsSearchResult.Status.Failure()) {
                    return DataActionResult<string>.Failed(
                        pathsSearchResult.Status
                    );
                }
                DataActionResult<string> newServicePathResult =
                    FindServicePath(
                        servicesData[key].strSearchKey,
                        pathsSearchResult.Result
                    );
                if(newServicePathResult.Status.Failure()) {
                    return DataActionResult<string>.Failed(
                        newServicePathResult.Status
                    );
                }
                servicesData[key].filePathCache = newServicePathResult.Result;
                path = newServicePathResult.Result;
            }
        }
        return DataActionResult<string>.Successful(path);
    }

    public StatusMessage ExecuteService(ExecutableServicesTypes serviceType, IPAddress address) {
        DataActionResult<string> pathResult = GetFilePath(serviceType);
        if(pathResult.Status.Failure()) {
            return pathResult.Status;
        }
        var psi = new ProcessStartInfo(
            pathResult.Result,
            address.ToString()
        ){
            UseShellExecute = true,
        };
        try {
            Process.Start(psi);
        }
        catch(Exception) {
            return StatusMessage.ExecutableServiceStartError;
        }
        return StatusMessage.Ok;
    }
}

}