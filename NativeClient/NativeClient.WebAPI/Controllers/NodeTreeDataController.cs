using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers {

[Route("api/nodes")]
public class NodeTreeDataController : BaseDataController {
    readonly INodeTreeDataService data;

    public NodeTreeDataController(
        INodeTreeDataService _data,
        IErrorReportAssemblerService _errAssembler
    ) : base(_errAssembler) {
        data = _data;
    }

    // GET api/nodes
    [HttpGet]
    public async Task<ActionResult> GetAllNodesData() {
        return ObserveDataOperationResult(
            await data.GetAllNodesData()
        );
    }
    
    // POST api/nodes/new
    [HttpPost("new")]
    public async Task<ActionResult> CreateNodeOnRoot([FromBody] NtwkNode node) {
        return ObserveDataOperationResult(
            await data.CreateNodeOnRoot(node)
        );
    }

    // POST api/nodes/1/new
    [HttpPost("{parentID:int}/new")]
    public async Task<ActionResult> CreateNodeWithParent(int parentID, [FromBody] NtwkNode node) {
        return ObserveDataOperationResult(
            await data.CreateNodeWithParent(node, parentID)
        );
    }

    // PUT api/nodes/1/setTags
    [HttpPut("{nodeID:int}/setTags")]
    public async Task<ActionResult> SetNodeTags(
        int nodeID,
        [FromBody] IEnumerable<int> tagsIDs
    ) {
        return ObserveDataOperationStatus(
            await data.SetNodeTags(nodeID, tagsIDs)
        );
    }

    // PUT api/nodes/1/changeParentTo/2
    [HttpPut("{nodeID:int}/changeParentTo/{newParentID:int}")]
    public async Task<ActionResult> MoveNodesSubtree(int nodeID, int newParentID) {
        return ObserveDataOperationStatus(
            await data.MoveNodesSubtree(nodeID, newParentID)
        );
    }

    // PUT api/nodes/1/update
    [HttpPut("{nodeID:int}/update")]
    public async Task<ActionResult> UpdateNode(int nodeID, [FromBody] NtwkNode node) {
        node.ID = nodeID;
        return ObserveDataOperationStatus(
            await data.UpdateNode(node)
        );
    }

    // DELETE api/nodes/1/delete
    [HttpDelete("{nodeID:int}/delete")]
    public async Task<ActionResult> RemoveNode(int nodeID) {
        return ObserveDataOperationResult(
            await data.RemoveNode(nodeID)
        );
    }
}

}