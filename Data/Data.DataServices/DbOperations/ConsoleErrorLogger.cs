using System;
using Data.Abstract.DbInteraction;

namespace Data.DataServices.DbOperations {

class ConsoleErrorLogger : IDbErrorLogger {
    public void LogError(string error) {
        Console.Out.WriteLine(error);
    }
}

}