using Data.Model.ResultsModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers
{
    public class BaseDataController : Controller
    {
        readonly IErrorReportAssemblerService _errAssembler;

        protected BaseDataController(IErrorReportAssemblerService errAssembler)
        {
            _errAssembler = errAssembler;
        }

        protected ActionResult ObserveDataOperationResult<T>(
            DataActionResult<T> dataResult
        )
        {
            if (dataResult.Status.Failure()) return BadRequest(_errAssembler.AssembleReport(dataResult.Status));
            return Ok(dataResult.Result);
        }

        protected ActionResult ObserveDataOperationStatus(
            StatusMessage status
        )
        {
            if (status.Failure()) return BadRequest(_errAssembler.AssembleReport(status));
            return NoContent();
        }
    }
}