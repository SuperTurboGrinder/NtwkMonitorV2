using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/webServices")]
public class WebServicesController : BaseDataController {
    readonly ICustomWebServicesDataService data;

    WebServicesController(ICustomWebServicesDataService _data) {
        data = _data;
    }

    // GET api/webServices
    [HttpGet]
    public async Task<ActionResult> GetAllCWS() {
        return await GetDbData(async () =>
            await data.GetAllCWS()
        );
    }

    // GET api/webServices/mapping
    [HttpGet("/mapping")]
    public async Task<ActionResult> GetCWSBondExistanceMapping() {
        return await GetDbData(async () =>
            await data.GetCWSBondExistanceMapping()
        );
    }

    // POST api/webServices/new
    [HttpPost("/new")]
    public async Task<ActionResult> CreateCustomWebService(CustomWebService cws) {
        return await GetDbData(async () =>
            await data.CreateCustomWebService(cws)
        );
    }

    // POST api/webServices/1/bind
    [HttpPost("/{webServiceID:int}/bind")]
    public async Task<ActionResult> CreateWebServiceBinding(
        int webServiceID,
        WebAPI.InputModel.WebServiceBinding bindingData
    ) {
        return await PerformDBOperation(async () =>
            await data.CreateWebServiceBinding(
                bindingData.nodeID,
                webServiceID,
                bindingData.param1,
                bindingData.param2,
                bindingData.param3
            )
        );
    }

    // PUT api/webServices/1/update
    [HttpPut("/{webServiceID:int}/update")]
    public async Task<ActionResult> UpdateCustomWebService(CustomWebService cws) {
        return await PerformDBOperation(async () =>
            await data.UpdateCustomWebService(cws)
        );
    }

    // PUT api/webServices/1/updateBinding
    [HttpPut("/{webServiceID:int}/updateBinding")]
    public async Task<ActionResult> UpdateWebServiceBinding(
        int webServiceID,
        WebAPI.InputModel.WebServiceBinding bindingData
    ) {
        return await PerformDBOperation(async () =>
            await data.UpdateWebServiceBinding(
                bindingData.nodeID,
                webServiceID,
                bindingData.param1,
                bindingData.param2,
                bindingData.param3
            )
        );
    }

    // DELETE api/webServices/1/delete
    [HttpDelete("/{webServiceID:int}/delete")]
    public async Task<ActionResult> RemoveCustomWebService(int webServiceID) {
        return await GetDbData(async () =>
            await data.RemoveCustomWebService(webServiceID)
        );
    }

    // DELETE api/webServices/1/deleteBinding
    [HttpDelete("/{webServiceID:int}/deleteBinding")]
    public async Task<ActionResult> RemoveWebServiceBinding(int webServiceID, int nodeID) {
        return await PerformDBOperation(async () =>
            await data.RemoveWebServiceBinding(nodeID, webServiceID)
        );
    }
}

}