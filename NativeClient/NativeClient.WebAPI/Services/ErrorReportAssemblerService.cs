using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Data.Model.ResultsModel;
using NativeClient.WebAPI.Services.Model;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Services {

//!!! singleton service
class ErrorReportAssemblerService : IErrorReportAssemblerService {
    Dictionary<StatusMessage, string> stringStatuses;

    public ErrorReportAssemblerService() {
        stringStatuses =
            ((StatusMessage[]) Enum.GetValues(typeof(StatusMessage)))
                .ToDictionary(
                    val => val,
                    val =>  {
                        string strVal = System.Text.RegularExpressions.Regex.Replace(
                            Enum.GetName(typeof(StatusMessage), val),
                            "(?<=[a-z])([A-Z])",
                            " $1",
                            System.Text.RegularExpressions.RegexOptions.Compiled
                        )
                        .Trim()
                        .ToLower();
                        return strVal.First().ToString().ToUpper()
                                +strVal.Substring(1)+".";
                    }
                );
    }

    public ErrorReport AssembleReport(StatusMessage status) =>
        new ErrorReport { status=status, statusString=stringStatuses[status] };
}

}