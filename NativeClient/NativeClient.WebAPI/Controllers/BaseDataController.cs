using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Abstract.DataAccessServices;
using Data.Model.ResultsModel;

namespace NativeClient.WebAPI.Controllers {

public class BaseDataController : Controller {

    protected async Task<ActionResult> GetDbData<T>(
        Func<Task<DataActionResult<T>>> getter
    ) {
        DataActionResult<T> dbDataResult = await getter();
        if(dbDataResult.Status.Failure()) {
            return BadRequest(dbDataResult.Status);
        }
        return Ok(dbDataResult.Result);
    }

    protected async Task<ActionResult> PerformDBOperation(
        Func<Task<StatusMessage>> operation
    ) {
        StatusMessage operationResultStatus = await operation();
        if(operationResultStatus.Failure()) {
            return BadRequest(operationResultStatus);
        }
        return Ok();
    }

}

}