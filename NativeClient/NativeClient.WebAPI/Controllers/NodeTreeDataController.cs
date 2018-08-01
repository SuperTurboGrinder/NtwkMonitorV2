using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/nodes")]
public class NodeTreeDataController : BaseDataController {
    readonly INodeTreeDataService data;

    NodeTreeDataController(INodeTreeDataService _data) {
        data = _data;
    }

    // GET api/nodes
    [HttpGet]
    public async Task<ActionResult> GetAllNodes() {
        return await GetDbData(async () =>
            await data.GetAllNodes()
        );
    }
    
    // POST api/nodes/new
    [HttpPost("/new")]
    public async Task<ActionResult> CreateNodeOnRoot(NtwkNode node) {
        return await GetDbData(async () =>
            await data.CreateNodeOnRoot(node)
        );
    }

    // POST api/nodes/new/1
    [HttpPost("/new/{parentID:int}")]
    public async Task<ActionResult> CreateNodeWithParent(int parentID, NtwkNode node) {
        return await GetDbData(async () =>
            await data.CreateNodeWithParent(node, parentID)
        );
    }

    // POST api/nodes/1/setTags
    [HttpPost("/{nodeID:int}/setTags")]
    public async Task<ActionResult> SetNodeTags(
        int nodeID,
        IEnumerable<int> tagsIDs
    ) {
        return await PerformDBOperation(async () =>
            await data.SetNodeTags(nodeID, tagsIDs)
        );
    }

    // PUT api/nodes/1/changeParent
    [HttpPut("/{nodeID:int}/changeParent")]
    public async Task<ActionResult> MoveNodesSubtree(int nodeID, int newParentID) {
        return await PerformDBOperation(async () =>
            await data.MoveNodesSubtree(nodeID, newParentID)
        );
    }

    // PUT api/nodes/1/update
    [HttpPut("/{nodeID:int}/update")]
    public async Task<ActionResult> UpdateNode(int nodeID, NtwkNode node) {
        node.ID = nodeID;
        return await PerformDBOperation(async () =>
            await data.UpdateNode(node)
        );
    }

    // DELETE api/nodes/1/delete
    [HttpDelete("/{nodeID:int}/delete")]
    public async Task<ActionResult> RemoveNode(int nodeID) {
        return await GetDbData(async () =>
            await data.RemoveNode(nodeID)
        );
    }
}

}