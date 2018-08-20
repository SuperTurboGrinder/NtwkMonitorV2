using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using NativeClient.WebAPI.Abstract;
using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/webServices")]
public class WebServicesController : BaseDataController {
    readonly ICustomWebServicesDataService data;

    public WebServicesController(
        ICustomWebServicesDataService _data,
        IErrorReportAssemblerService _errAssembler
    ) : base(_errAssembler) {
        data = _data;
    }

    // GET api/webServices
    [HttpGet]
    public async Task<ActionResult> GetAllCWS() {
        return ObserveDataOperationResult(
            await data.GetAllCWS()
        );
    }

    // POST api/webServices/new
    [HttpPost("new")]
    public async Task<ActionResult> CreateCustomWebService([FromBody] CustomWebService cws) {
        return ObserveDataOperationResult(
            await data.CreateCustomWebService(cws)
        );
    }

    // POST api/webServices/1/bind
    [HttpPost("{webServiceID:int}/bind")]
    public async Task<ActionResult> CreateWebServiceBinding(
        int webServiceID,
        [FromBody] WebAPI.InputModel.WebServiceBinding bindingData
    ) {
        return ObserveDataOperationStatus(
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
    [HttpPut("{webServiceID:int}/update")]
    public async Task<ActionResult> UpdateCustomWebService(int webServiceID, [FromBody] CustomWebService cws) {
        cws.ID = webServiceID;
        return ObserveDataOperationStatus(
            await data.UpdateCustomWebService(cws)
        );
    }

    // PUT api/webServices/1/updateBinding
    [HttpPut("{webServiceID:int}/updateBinding")]
    public async Task<ActionResult> UpdateWebServiceBinding(
        int webServiceID,
        [FromBody] WebAPI.InputModel.WebServiceBinding bindingData
    ) {
        return ObserveDataOperationStatus(
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
    [HttpDelete("{webServiceID:int}/delete")]
    public async Task<ActionResult> RemoveCustomWebService(int webServiceID) {
        return ObserveDataOperationResult(
            await data.RemoveCustomWebService(webServiceID)
        );
    }

    // DELETE api/webServices/1/deleteBindingWithNode/1
    [HttpDelete("{webServiceID:int}/deleteBindingWithNode/{nodeID:int}")]
    public async Task<ActionResult> RemoveWebServiceBinding(int webServiceID, int nodeID) {
        return ObserveDataOperationStatus(
            await data.RemoveWebServiceBinding(nodeID, webServiceID)
        );
    }
}

}