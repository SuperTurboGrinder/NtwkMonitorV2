using System;
using Data.Abstract.DbInteraction;

namespace Data.DataServices.DbOperations
{
    public class ConsoleErrorLogger : IDbErrorLogger
    {
        public void LogException(Exception ex)
        {
            string separator = new string('_', 8);
            Console.Out.WriteLine(separator + "Error" + separator);
            Console.Out.WriteLine(ex.ToString());
            Exception innerEx = ex.InnerException;
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (innerEx != null)
            {
                Console.Out.WriteLine(innerEx.ToString());
            }
        }
    }
}