using Data.Model.ResultsModel;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace NativeClient.WebAPI.Services.Model
{
    public class ErrorReport
    {
        public StatusMessage Status { get; set; }
        public string StatusString { get; set; }
    }
}