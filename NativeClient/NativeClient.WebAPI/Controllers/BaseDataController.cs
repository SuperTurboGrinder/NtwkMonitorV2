using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

public class BaseDataController : Controller {

    protected async Task<ActionResult> GetDbData<T>(Func<Task<DataActionResult<T>>> getter) {
        DataActionResult<T> rawData = await getter();
        if(!rawData.Success) {
            return BadRequest(rawData.Error);
        }
        return Ok(rawData.Result);
    }

    protected async Task<ActionResult> PerformDBOperation(Func<Task<DataActionVoidResult>> getter) {
        DataActionVoidResult rawData = await getter();
        if(!rawData.Success) {
            return BadRequest(rawData.Error);
        }
        return Ok();
    }

}

}