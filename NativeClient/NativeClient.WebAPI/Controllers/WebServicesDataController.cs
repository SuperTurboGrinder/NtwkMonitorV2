using System.Threading.Tasks;
using Data.Abstract.DataAccessServices;
using Data.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers
{
    [Route("api/webServices")]
    public class WebServicesController : BaseDataController
    {
        readonly ICustomWebServicesDataService _data;

        public WebServicesController(
            ICustomWebServicesDataService data,
            IErrorReportAssemblerService errAssembler
        ) : base(errAssembler)
        {
            _data = data;
        }

        // GET api/webServices
        [HttpGet]
        public async Task<ActionResult> GetAllCws()
        {
            return ObserveDataOperationResult(
                await _data.GetAllCws()
            );
        }

        // POST api/webServices/new
        [HttpPost("new")]
        public async Task<ActionResult> CreateCustomWebService([FromBody] CustomWebService cws)
        {
            return ObserveDataOperationResult(
                await _data.CreateCustomWebService(cws)
            );
        }

        // POST api/webServices/1/bind
        [HttpPost("{webServiceID:int}/bind")]
        public async Task<ActionResult> CreateWebServiceBinding(
            int webServiceId,
            [FromBody] InputModel.WebServiceBinding bindingData
        )
        {
            return ObserveDataOperationStatus(
                await _data.CreateWebServiceBinding(
                    bindingData.NodeId,
                    webServiceId,
                    bindingData.Param1,
                    bindingData.Param2,
                    bindingData.Param3
                )
            );
        }

        // PUT api/webServices/1/update
        [HttpPut("{webServiceID:int}/update")]
        public async Task<ActionResult> UpdateCustomWebService(int webServiceId, [FromBody] CustomWebService cws)
        {
            cws.Id = webServiceId;
            return ObserveDataOperationStatus(
                await _data.UpdateCustomWebService(cws)
            );
        }

        // PUT api/webServices/1/updateBinding
        [HttpPut("{webServiceID:int}/updateBinding")]
        public async Task<ActionResult> UpdateWebServiceBinding(
            int webServiceId,
            [FromBody] InputModel.WebServiceBinding bindingData
        )
        {
            return ObserveDataOperationStatus(
                await _data.UpdateWebServiceBinding(
                    bindingData.NodeId,
                    webServiceId,
                    bindingData.Param1,
                    bindingData.Param2,
                    bindingData.Param3
                )
            );
        }

        // DELETE api/webServices/1/delete
        [HttpDelete("{webServiceID:int}/delete")]
        public async Task<ActionResult> RemoveCustomWebService(int webServiceId)
        {
            return ObserveDataOperationResult(
                await _data.RemoveCustomWebService(webServiceId)
            );
        }

        // DELETE api/webServices/1/deleteBindingWithNode/1
        [HttpDelete("{webServiceID:int}/deleteBindingWithNode/{nodeID:int}")]
        public async Task<ActionResult> RemoveWebServiceBinding(int webServiceId, int nodeId)
        {
            return ObserveDataOperationStatus(
                await _data.RemoveWebServiceBinding(nodeId, webServiceId)
            );
        }
    }
}