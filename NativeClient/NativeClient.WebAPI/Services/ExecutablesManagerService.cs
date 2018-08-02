using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services {

class ExecutablesManagerService {
    Dictionary<string, string> serviceFilesPaths;

    struct FilePathData {
        public string fullPath;
        public string filename;
    }

    public ExecutablesManagerService() {
        serviceFilesPaths["ssh"] = null;
        serviceFilesPaths["telnet"] = null;
    }

    string FindAndSetPath(string key, FilePathData[] paths) {
        IEnumerable<FilePathData> candidates =
            paths.Where(f => f.filename.Contains(key));
        if(candidates.Count() == 0) {
            return $"No {key} service found in bin directory.";
        }
        else if(candidates.Count() > 1) {
            return $"Too many candidates for {key} service in bin directory.";
        }
        serviceFilesPaths[key] = candidates.Single().fullPath;
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
        string error = FindAndSetPath("ssh", files);
        if(error != null) return error;

        FindAndSetPath("telnet", files);
        if(error != null) return error;

        return null;
    }

    string GetFilePath(string key, out string error) {
        string path = serviceFilesPaths[key];
        error = null;
        if(path == null || !File.Exists(path)) {
            error = UpdateFilePaths();
            if(error != null) {
                return null;
            }
        }
        path = serviceFilesPaths[key];
        return path;
    }
}

}