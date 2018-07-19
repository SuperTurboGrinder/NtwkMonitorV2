using System;
using Data.Abstract.DbInteraction;

namespace Data.DataServices.DbOperations {

class ConsoleErrorLogger : IDbErrorLogger {
    public void LogException(Exception ex) {
        string separator = new string('_', 8);
        Console.Out.WriteLine(separator+"Error"+separator);
        Console.Out.WriteLine(ex.ToString());
        Exception innerEx = ex.InnerException;
        while(innerEx != null) {
            Console.Out.WriteLine(innerEx.ToString());
        }
    }
}

}