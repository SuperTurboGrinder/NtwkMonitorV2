using System;
using System.Collections.Generic;
using System.Linq;
using Data.Model.ResultsModel;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services
{
//!!! singleton service
    class ErrorReportAssemblerService : IErrorReportAssemblerService
    {
        readonly Dictionary<StatusMessage, string> _stringStatuses;

        public ErrorReportAssemblerService()
        {
            _stringStatuses =
                ((StatusMessage[]) Enum.GetValues(typeof(StatusMessage)))
                .ToDictionary(
                    val => val,
                    val =>
                    {
                        string strVal = System.Text.RegularExpressions.Regex.Replace(
                                Enum.GetName(typeof(StatusMessage), val),
                                "(?<=[a-z])([A-Z])",
                                " $1",
                                System.Text.RegularExpressions.RegexOptions.Compiled
                            )
                            .Trim()
                            .ToLower();
                        return strVal.First().ToString().ToUpper()
                               + strVal.Substring(1) + ".";
                    }
                );
        }

        public ErrorReport AssembleReport(StatusMessage status)
        {
            return new ErrorReport {Status = status, StatusString = _stringStatuses[status]};
        }
    }
}