using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Abstract.DataAccessServices;
using Data.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers
{
    [Route("api/nodes")]
    public class NodeTreeDataController : BaseDataController
    {
        readonly INodeTreeDataService _data;

        public NodeTreeDataController(
            INodeTreeDataService data,
            IErrorReportAssemblerService errAssembler
        ) : base(errAssembler)
        {
            _data = data;
        }

        // GET api/nodes
        [HttpGet]
        public async Task<ActionResult> GetAllNodesData()
        {
            return ObserveDataOperationResult(
                await _data.GetAllNodesData()
            );
        }

        // POST api/nodes/new
        [HttpPost("new")]
        public async Task<ActionResult> CreateNodeOnRoot([FromBody] NtwkNode node)
        {
            return ObserveDataOperationResult(
                await _data.CreateNodeOnRoot(node)
            );
        }

        // POST api/nodes/1/new
        [HttpPost("{parentID:int}/new")]
        public async Task<ActionResult> CreateNodeWithParent(int parentId, [FromBody] NtwkNode node)
        {
            return ObserveDataOperationResult(
                await _data.CreateNodeWithParent(node, parentId)
            );
        }

        // PUT api/nodes/1/setTags
        [HttpPut("{nodeID:int}/setTags")]
        public async Task<ActionResult> SetNodeTags(
            int nodeId,
            [FromBody] IEnumerable<int> tagsIDs
        )
        {
            return ObserveDataOperationStatus(
                await _data.SetNodeTags(nodeId, tagsIDs)
            );
        }

        // PUT api/nodes/1/changeParentTo/2
        [HttpPut("{nodeID:int}/changeParentTo/{newParentID:int}")]
        public async Task<ActionResult> MoveNodesSubtree(int nodeId, int newParentId)
        {
            return ObserveDataOperationStatus(
                await _data.MoveNodesSubtree(nodeId, newParentId)
            );
        }

        // PUT api/nodes/1/update
        [HttpPut("{nodeID:int}/update")]
        public async Task<ActionResult> UpdateNode(int nodeId, [FromBody] NtwkNode node)
        {
            node.Id = nodeId;
            return ObserveDataOperationStatus(
                await _data.UpdateNode(node)
            );
        }

        // DELETE api/nodes/1/delete
        [HttpDelete("{nodeID:int}/delete")]
        public async Task<ActionResult> RemoveNode(int nodeId)
        {
            return ObserveDataOperationResult(
                await _data.RemoveNode(nodeId)
            );
        }
    }
}