using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services {

//SINGLETON SERVICE
class ExecutablesManagerService {
    class ServiceData {
        public string filePath;
        public string strSearchKey;
        //public int port;
    }

    Dictionary<ExecutableServicesTypes, ServiceData> servicesData;

    struct FilePathData {
        public string fullPath;
        public string filename;
    }

    public ExecutablesManagerService() {
        lock(servicesData) {
            servicesData[ExecutableServicesTypes.SSH] = new ServiceData();
            servicesData[ExecutableServicesTypes.Telnet] = new ServiceData();
            servicesData[ExecutableServicesTypes.SSH].strSearchKey = "ssh";
            //servicesData[ExecutableServicesTypes.SSH].port = 22;
            servicesData[ExecutableServicesTypes.Telnet].strSearchKey = "telnet";
            //servicesData[ExecutableServicesTypes.Telnet].port = 23;
        }
    }

    string FindAndSetPath(ExecutableServicesTypes key, FilePathData[] paths) {
        string strKey = servicesData[key].strSearchKey;
        IEnumerable<FilePathData> candidates =
            paths.Where(f => f.filename.Contains(strKey));
        if(candidates.Count() == 0) {
            return $"No {strKey} service found in bin directory.";
        }
        else if(candidates.Count() > 1) {
            return $"Too many candidates for {strKey} service in bin directory.";
        }
        servicesData[key].filePath = candidates.Single().fullPath;
        return null;
    }

    string UpdateFilePaths() {
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
            return "Not enough OS rights to search for executable services.";
        }
        catch(DirectoryNotFoundException) {
            return "Could not find bin directory while searching for executable services.";
        }
        catch(IOException) {
            return "Unkonown IO error while trying to find executable services.";
        }
        string error = FindAndSetPath(ExecutableServicesTypes.SSH, files);
        if(error != null) return error;

        error = FindAndSetPath(ExecutableServicesTypes.Telnet, files);
        if(error != null) return error;

        return null;
    }

    string GetFilePath(ExecutableServicesTypes key, out string error) {
        string path = null;
        lock(servicesData) {
            path = servicesData[key].filePath;
            error = null;
            if(path == null || !File.Exists(path)) {
                error = UpdateFilePaths();
                if(error != null) {
                    return null;
                }
            }
            path = servicesData[key].filePath;
        }
        return path;
    }

    public string ExecuteService(ExecutableServicesTypes serviceType, IPAddress address) {
        string executableSearchError = null;
        string path = GetFilePath(serviceType, out executableSearchError);
        if(executableSearchError != null) {
            return executableSearchError;
        }
        var psi = new ProcessStartInfo(
            path,
            address.ToString()
        ){
            UseShellExecute = true,
        };
        try {
            Process.Start(psi);
        }
        catch(Exception) {
            return $"Executable service start error.\n({path})";
        }
        return null;
    }
}

}