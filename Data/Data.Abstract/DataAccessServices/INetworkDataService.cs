using System.Collections.Generic;
using System;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through IDataActionResult
public interface INetworkDataService {
    IDataActionResult<IEnumerable<Network>> GetAllNetworks();
    IDataActionResult<IEnumerable<NtwkNode>> GetNetworkNodes(int networkID);
    IDataActionResult<IEnumerable<NtwkNode>> GetTaggedNodes(int tagID);
    IDataActionResult<IEnumerable<NodeTag>> GetNetworkTags(int networkID);
    IDataActionResult<IEnumerable<NodeTag>> GetUnboundTags();
    IDataActionResult<IEnumerable<CustomWebService>> GetAllCWS();

    IDataActionResult<Network> CreateNetwork(Network network);
    IDataActionVoidResult SetAsDefaultNetwork(int networkID);
    IDataActionResult<NtwkNode> CreateNode(NtwkNode node);
    IDataActionResult<NodeTag> CreateTag(NodeTag tag);
    IDataActionResult<CustomWebService> CreateCustomWebService(
        CustomWebService cws);

    IDataActionVoidResult UpdateNetwork(Network network);
    IDataActionVoidResult UpdateNode(NtwkNode node);
    IDataActionVoidResult UpdateCustomWebService(
        CustomWebService cws);

    IDataActionResult<Network> RemoveNetwork(int networkID);
    IDataActionResult<NtwkNode> RemoveNode(int nodeID);
    IDataActionResult<NodeTag> RemoveTag(int nodeID);
    IDataActionResult<CustomWebService> RemoveCustomWebService(int cwsID);
}

}