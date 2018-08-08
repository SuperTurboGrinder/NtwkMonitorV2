using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using NativeClient.WebAPI.Services.Model;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Services {

//SINGLETON SERVICE
class DefaultWebServiceLauncherService : IWebServiceLauncherService {
    public string Start(string uri) {
        var psi = new ProcessStartInfo() {
            UseShellExecute = true,
        };
        try { //use default browser
            Process.Start(psi);
        }
        catch(Exception) {
            return $"Custom web service start error.";
        }
        return null;
    }
}

}