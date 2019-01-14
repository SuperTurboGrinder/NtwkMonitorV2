using System;
using System.Globalization;
using System.IO;
using Data.Abstract.DbInteraction;

namespace Data.DataServices.DbOperations
{
    public class FileErrorLogger : IDbErrorLogger
    {
        public void LogException(Exception ex)
        {
            string datetime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            using (StreamWriter file = new StreamWriter("log.txt", true))
            {
                file.WriteLine();
                file.WriteLine();
                file.WriteLine(datetime);
                file.Write(ex.ToString());
                Exception innerEx = ex.InnerException;
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (innerEx != null)
                {
                    Console.Out.WriteLine(innerEx.ToString());
                }
            }

            Console.WriteLine(datetime);
            Console.WriteLine("Database operation exception logged to file...");
        }
    }
}