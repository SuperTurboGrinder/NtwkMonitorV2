using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using NativeClient.WebAPI.Abstract;
using Data.Abstract.DataAccessServices;
using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Controllers {

public class BaseDataController : Controller {
    IErrorReportAssemblerService errAssembler;

    protected  BaseDataController(IErrorReportAssemblerService _errAssembler) {
        errAssembler = _errAssembler;
    }

    protected ActionResult ObserveDataOperationResult<T>(
        DataActionResult<T> dataResult
    ) {
        if(dataResult.Status.Failure()) {
            return BadRequest(errAssembler.AssembleReport(dataResult.Status));
        }
        return Ok(dataResult.Result);
    }

    protected ActionResult ObserveDataOperationStatus(
        StatusMessage status
    ) {
        if(status.Failure()) {
            return BadRequest(errAssembler.AssembleReport(status));
        }
        return NoContent();
    }

}

}