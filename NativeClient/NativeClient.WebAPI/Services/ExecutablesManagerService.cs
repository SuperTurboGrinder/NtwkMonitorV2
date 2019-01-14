using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Data.Model.ResultsModel;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services
{
//SINGLETON SERVICE
    public class ExecutablesManagerService : IExecutablesManagerService
    {
        readonly Dictionary<ExecutableServicesTypes, ServiceData> _servicesData =
            new Dictionary<ExecutableServicesTypes, ServiceData>();

        public ExecutablesManagerService()
        {
            lock (_servicesData)
            {
                _servicesData[ExecutableServicesTypes.Ssh] = new ServiceData();
                _servicesData[ExecutableServicesTypes.Telnet] = new ServiceData();
                _servicesData[ExecutableServicesTypes.Ssh].StrSearchKey = "ssh";
                _servicesData[ExecutableServicesTypes.Telnet].StrSearchKey = "telnet";
            }
        }

        public StatusMessage ExecuteService(ExecutableServicesTypes serviceType, IPAddress address)
        {
            DataActionResult<string> pathResult = GetFilePath(serviceType);
            if (pathResult.Status.Failure()) return pathResult.Status;
            var psi = new ProcessStartInfo(
                pathResult.Result,
                address.ToString()
            )
            {
                UseShellExecute = true,
            };
            try
            {
                Process.Start(psi);
            }
            catch (Exception)
            {
                return StatusMessage.ExecutableServiceStartError;
            }

            return StatusMessage.Ok;
        }

        static DataActionResult<string> FindServicePath(
            string strKey,
            FilePathData[] paths
        )
        {
            IEnumerable<FilePathData> candidates = paths
                .Where(f => f.Filename.Contains(strKey))
                .ToArray();
            if (!candidates.Any())
                return DataActionResult<string>.Failed(
                    StatusMessage.NoSpecifiedExecServiceFoundInBinDirectory
                );
            else if (candidates.Count() > 1)
                return DataActionResult<string>.Failed(
                    StatusMessage.TooManyCandidatesForSpecefiedExecServiceInBinDirectory
                );
            return DataActionResult<string>.Successful(candidates.Single().FullPath);
        }

        static DataActionResult<FilePathData[]> SearchBinDirectoryForFilePaths()
        {
            FilePathData[] files;
            try
            {
                files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"/bin")
                    .Select(f => new FilePathData()
                    {
                        FullPath = f,
                        Filename = Path.GetFileName(f)
                    })
                    .ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                return DataActionResult<FilePathData[]>.Failed(
                    StatusMessage.NotEnoughOsRightsToSearchForExecServices
                );
            }
            catch (DirectoryNotFoundException)
            {
                return DataActionResult<FilePathData[]>.Failed(
                    StatusMessage.CouldNotFindBinDirectoryWhileSearchingForExecutableServices
                );
            }
            catch (IOException)
            {
                return DataActionResult<FilePathData[]>.Failed(
                    StatusMessage.UnknownIoErrorWhileTryingToFindExecutableServices
                );
            }

            return DataActionResult<FilePathData[]>.Successful(files);
        }

        DataActionResult<string> GetFilePath(ExecutableServicesTypes key)
        {
            string path;
            lock (_servicesData)
            {
                path = _servicesData[key].FilePathCache;
                if (path == null || !File.Exists(path))
                {
                    DataActionResult<FilePathData[]> pathsSearchResult =
                        SearchBinDirectoryForFilePaths();
                    if (pathsSearchResult.Status.Failure())
                        return DataActionResult<string>.Failed(
                            pathsSearchResult.Status
                        );
                    DataActionResult<string> newServicePathResult =
                        FindServicePath(
                            _servicesData[key].StrSearchKey,
                            pathsSearchResult.Result
                        );
                    if (newServicePathResult.Status.Failure())
                        return DataActionResult<string>.Failed(
                            newServicePathResult.Status
                        );
                    _servicesData[key].FilePathCache = newServicePathResult.Result;
                    path = newServicePathResult.Result;
                }
            }

            return DataActionResult<string>.Successful(path);
        }

        class ServiceData
        {
            public string FilePathCache;
            public string StrSearchKey;
        }

        struct FilePathData
        {
            public string FullPath;
            public string Filename;
        }
    }
}