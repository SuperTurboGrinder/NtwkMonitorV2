using System;
using System.IO;
using Data.Abstract.DbInteraction;

namespace Data.DataServices.DbOperations {

public class FileErrorLogger : IDbErrorLogger {
    public void LogException(Exception ex) {
        string datetime = DateTime.Now.ToString();
        using (StreamWriter file = new StreamWriter("log.txt", true)) {
            file.WriteLine();
            file.WriteLine();
            file.WriteLine(datetime);
            file.Write(ex.ToString());
            Exception innerEx = ex.InnerException;
            while(innerEx != null) {
                Console.Out.WriteLine(innerEx.ToString());
            }
        }
        Console.WriteLine(datetime);
        Console.WriteLine("Database operation exception logged to file...");
    }
}

}