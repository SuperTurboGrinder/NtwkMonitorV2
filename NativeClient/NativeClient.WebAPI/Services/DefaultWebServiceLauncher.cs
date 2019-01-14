using System;
using System.Diagnostics;
using Data.Model.ResultsModel;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Services
{
//SINGLETON SERVICE
    class DefaultWebServiceLauncherService : IWebServiceLauncherService
    {
        public StatusMessage Start(string uri)
        {
            var psi = new ProcessStartInfo(uri)
            {
                UseShellExecute = true,
            };
            try
            {
                //use default browser
                Process.Start(psi);
            }
            catch (Exception)
            {
                return StatusMessage.CustomWebServiceStartError;
            }

            return StatusMessage.Ok;
        }
    }
}