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

    // GET api/network/nodes
    [HttpGet("/nodes")]
    public async Task<ActionResult> GetAllNodes() {
        return await GetDbData(async () =>
            await data.GetAllNodes()
        );
    }

    // GET api/network/webServices
    //[HttpGet("/nodes/withTag/{tagID:int}")]
    //public async Task<ActionResult> GetTaggedNodesIDs(int tagID) {
    //    return await GetDbData(async () =>
    //        await data.GetTaggedNodesIDs(tagID)
    //    );
    //}

    
    // POST api/network/nodes/new
    [HttpPost("/nodes/new")]
    public async Task<ActionResult> CreateNodeOnRoot(NtwkNode node) {
        return await GetDbData(async () =>
            await data.CreateNodeOnRoot(node)
        );
    }

    // POST api/network/nodes/new/1
    [HttpPost("/nodes/new/{parentID:int}")]
    public async Task<ActionResult> CreateNodeWithParent(int parentID, NtwkNode node) {
        return await GetDbData(async () =>
            await data.CreateNodeWithParent(node, parentID)
        );
    }

    // POST api/network/nodes/1/setTags
    [HttpPost("/nodes/{nodeID:int}/setTags")]
    public async Task<ActionResult> SetNodeTags(
        int nodeID,
        IEnumerable<int> tagsIDs
    ) {
        return await PerformDBOperation(async () =>
            await data.SetNodeTags(nodeID, tagsIDs)
        );
    }

    // PUT api/network/nodes/1/changeParent
    [HttpPut("/nodes/{nodeID:int}/changeParent")]
    public async Task<ActionResult> MoveNodesSubtree(int nodeID, int newParentID) {
        return await PerformDBOperation(async () =>
            await data.MoveNodesSubtree(nodeID, newParentID)
        );
    }

    // PUT api/network/nodes/1/update
    [HttpPut("/nodes/{nodeID:int}/update")]
    public async Task<ActionResult> UpdateNode(int nodeID, NtwkNode node) {
        node.ID = nodeID;
        return await PerformDBOperation(async () =>
            await data.UpdateNode(node)
        );
    }

    // DELETE api/network/nodes/1/delete
    [HttpDelete("/nodes/{nodeID:int}/delete")]
    public async Task<ActionResult> RemoveNode(int nodeID) {
        return await GetDbData(async () =>
            await data.RemoveNode(nodeID)
        );
    }
}

}