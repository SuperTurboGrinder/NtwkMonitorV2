using System.Collections.Generic;
using System;

using Data.Model.ViewModel;

namespace Data.Abstract.DbInteraction {

//logging db operations exceptions from lower level IDbDataSource
//reporting status but not the error through IDbOperationResult
public interface IDataRopository {
    IDbOperationResult<MonitoringSession> GetNewSession(int tagID);
    IDbOperationResult<bool> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult
    );
    IDbOperationVoidResult ClearEmptySessions();
    IDbOperationVoidResult ClearOldSessions();

    IDbOperationResult<IEnumerable<MonitoringSession>> GetSessionsFor(int networkID);
    IDbOperationResult<IEnumerable<MonitoringPulseResult>> GetPulsesFor(int monitoringSessionID);

    IDbOperationResult<IEnumerable<Network>> GetAllNetworks();
    IDbOperationResult<IEnumerable<NtwkNode>> GetNetworkNodes(int networkID);
    IDbOperationResult<IEnumerable<NtwkNode>> GetTaggedNodes(int tagID);
    IDbOperationResult<IEnumerable<NodeTag>> GetNetworkTags(int networkID);
    IDbOperationResult<IEnumerable<NodeTag>> GetUnboundTags();
    IDbOperationResult<IEnumerable<CustomWebService>> GetAllCVS();
    

    IDbOperationResult<Network> CreateNetwork(Network network);
    IDbOperationVoidResult SetAsDefaultNetwork(int networkID);
    IDbOperationResult<NtwkNode> CreateNode(NtwkNode node);
    IDbOperationResult<NodeTag> CreateTag(NodeTag tag);
    IDbOperationResult<CustomWebService> CreateCustomWebService(
        CustomWebService cws);

    IDbOperationVoidResult UpdateNetwork(Network network);
    IDbOperationVoidResult UpdateNode(NtwkNode node);
    IDbOperationVoidResult UpdateCustomWebService(CustomWebService cws);

    IDbOperationResult<Network> RemoveNetwork(int networkID);
    IDbOperationResult<NtwkNode> RemoveNode(int nodeID);
    IDbOperationResult<NodeTag> RemoveTag(int nodeID);
    IDbOperationResult<CustomWebService> RemoveCustomWebService(int cwsID);
}

}