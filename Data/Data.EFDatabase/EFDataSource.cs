using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.Abstract.DbInteraction;

namespace Data.EFDatabase {

public class EFDataSource : IEFDbDataSource {
    readonly NtwkDBContext context;
    readonly IDbErrorLogger errorLogger;

    public EFDataSource(NtwkDBContext _context, IDbErrorLogger _errorLogger) {
        context = _context;
        errorLogger = _errorLogger;
    }

    private static double JSDateTimeNow() {
        return DateTime.UtcNow
               .Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc))
               .TotalMilliseconds;
    }

    public async Task<MonitoringSession> GetNewSession(
        int profileID
    ) {
        int monitoredNodesNum = await context.ProfilesTagSelection
            .Where(pts => pts.BindedProfileID == profileID &&
                          ProfileSelectedTagFlags.Monitor ==
                            (pts.Flags & ProfileSelectedTagFlags.Monitor))
            .Include(pts => pts.Tag)
                .ThenInclude(t => t.Attachments)
            .SelectMany(pts => pts.Tag.Attachments.Select(ta => ta.NodeID))
            .Distinct()
            .CountAsync();
        MonitoringSession newSession = new MonitoringSession {
            ID = 0,
            CreatedByProfileID = profileID,
            ParticipatingNodesNum = monitoredNodesNum,
            CreationTime = JSDateTimeNow(),
            LastPulseTime = 0,
        };
        context.MonitoringSessions.Add(newSession);
        await context.SaveChangesAsync();
        context.Entry(newSession).State = EntityState.Detached;
        return newSession;
    }

    public async Task<MonitoringPulseResult> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    ) {
        pulseResult.CreationTime = JSDateTimeNow();
        MonitoringSession session = await context.MonitoringSessions
            .Include(s => s.Pulses)
            .SingleAsync(s => s.ID == sessionID);
        context.MonitoringPulses
            .Add(pulseResult);
        await context.SaveChangesAsync();
        session.Pulses.Add(pulseResult);
        await context.SaveChangesAsync();
        foreach(MonitoringMessage message in messages) {
            context.MonitoringMessages
                .Add(message);
        }
        await context.SaveChangesAsync();
        MonitoringPulseResult pulseWithMessages = await context.MonitoringPulses
            .Include(p => p.Messages)
            .SingleAsync(p => p.ID == pulseResult.ID);
        foreach(MonitoringMessage messageEntity in messages) {
            pulseWithMessages.Messages.Add(messageEntity);
        }
        await context.SaveChangesAsync();
        return pulseResult;
    }

    public async Task ClearEmptySessions() {
        IQueryable<MonitoringSession> EmptySessions = context.MonitoringSessions
            .Where(s => s.LastPulseTime == 0);
        context.MonitoringSessions.RemoveRange(EmptySessions);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MonitoringSession>> GetSessionsForProfile(int profileID) {
        return await context.MonitoringSessions
            .AsNoTracking()
            .Where(s => s.CreatedByProfileID == profileID)
            .OrderByDescending(s => s.CreationTime)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<MonitoringPulseResult>> GetSessionReport(int monitoringSessionID) {
        return (await context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.Pulses)
            .SingleAsync(s => s.ID == monitoringSessionID))
            .Pulses;
    }


    public async Task<bool> HasChildren(int nodeID) {
        int nodesWithThisAncestor = await context.NodesClosureTable
            .Where(c => c.AncestorID == nodeID)
            .CountAsync();
        return nodesWithThisAncestor != 1; //only self ref closure
    }

    public async Task<bool> CheckIfProfileExists(int profileID) {
        return await context.Profiles.AnyAsync(n => n.ID == profileID);
    }

    private static int ParamNum(CustomWebService cws) {
        return (cws.Parametr3Name != null) ? (3) : (
            (cws.Parametr2Name != null) ? (2) : (
                (cws.Parametr1Name != null) ? (1) : (0)
            )
        );
    }

    public async Task<int> GetCWSParamNumber(int cwsID) {
        CustomWebService cws = await context.WebServices
            .SingleOrDefaultAsync(ws => ws.ID == cwsID);
        return (cws == null) ? (-1) : (
            ParamNum(cws)
        );
    }

    public async Task<bool> CheckIfSessionExists(int sessionID) {
        return await context.MonitoringSessions.AnyAsync(n => n.ID == sessionID);
    }

    public async Task<bool> CheckIfCWSBindingExists(int cwsID, int nodeID) {
        return await context.WebServiceBindings
            .AnyAsync(wsb =>
                wsb.NodeID == nodeID &&
                wsb.ServiceID == cwsID
            );
    }

    public async Task<bool> CheckIfTagExists(int tagID) {
        return await context.Tags.AnyAsync(n => n.ID == tagID);
    }

    public async Task<bool> CheckIfTagsExist(IEnumerable<int> tagsIDs) {
        return await context.Tags
            .Where(t => tagsIDs.Contains(t.ID))
            .CountAsync()
            == tagsIDs.Count();
    }
    
    public async Task<bool> CheckIfNodeExists(int nodeID) {
        return await context.Nodes.AnyAsync(n => n.ID == nodeID);
    }

    public async Task<bool> CheckIfTagNameExists(string name, int? exceptID) {
        return (
                   exceptID == null
                   || (await context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.ID == exceptID))?.Name != name
               ) && await context.Tags.AsNoTracking()
                   .Where(e => e.Name == name)
                   .AnyAsync();
    }

    public async Task<bool> CheckIfNodeNameExists(string name, int? exceptID) {
        return (
                   exceptID == null
                   || (await context.Nodes.AsNoTracking().FirstOrDefaultAsync(n => n.ID == exceptID))?.Name != name
               ) && await context.Nodes.AsNoTracking()
                   .Where(e => e.Name == name)
                   .AnyAsync();
    }

    public async Task<bool> CheckIfCWSNameExists(string name, int? exceptID) {
        return (
                   exceptID == null
                   || (await context.WebServices.AsNoTracking().FirstOrDefaultAsync(ws => ws.ID == exceptID))?.Name != name
               ) && await context.WebServices.AsNoTracking()
                   .Where(e => e.Name == name)
                   .AnyAsync();
    }

    public async Task<bool> CheckIfProfileNameExists(string name, int? exceptID) {
        return (
                   exceptID == null
                   || (await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.ID == exceptID))?.Name != name
               ) && await context.Profiles.AsNoTracking()
                   .Where(e => e.Name == name)
                   .AnyAsync();
    }

    // building tree structure for node
    // public for testing purpuses
    // should not be used outside this class
    public static async Task<List<NodeClosure>> __CreateNewNodeClosures(
        NtwkDBContext context,
        int? parentID,
        int nodeID
    ) {
        List<NodeClosure> result;
        if(parentID != null) {
            result = await context.NodesClosureTable
                .AsNoTracking()
                .Where(c => c.DescendantID == parentID)
                .Select(c => new NodeClosure {
                    ID = 0,
                    AncestorID = c.AncestorID,
                    DescendantID = nodeID,
                    Distance = c.Distance+1
                })
                .ToListAsync();
            result.Add(new NodeClosure {
                ID = 0,
                AncestorID = nodeID,
                DescendantID = nodeID,
                Distance = 0
            });
        }
        else {
            result = new [] {
                new NodeClosure {
                    ID = 0,
                    AncestorID = null,
                    DescendantID = nodeID,
                    Distance = 0
                },
                new NodeClosure {
                    ID = 0,
                    AncestorID = nodeID,
                    DescendantID = nodeID,
                    Distance = 0
                }
            }.ToList();
        }
        return result;
    }

    // CYCLE PREVENTION LOGIC
    public async Task<bool> CheckIfNodeInSubtree(
        int nodeID,
        int subtreeRootNodeID
    ) {
        return await context.NodesClosureTable
            .AnyAsync(c => 
                c.AncestorID == subtreeRootNodeID &&
                c.DescendantID == nodeID
            );
    }
    
    // Should be checked by higher level
    // services for newParent not being part of node's subtree.
    // Sidenote. Worst implementation ever
    public async Task MoveNodesSubtree(int nodeID, int newParentID) {
        int? newParentIDOrNull = newParentID == 0 ? (int?)null : newParentID;
        IQueryable<int> subtreeNodesIDs = context.NodesClosureTable
            .Where(c => c.AncestorID == nodeID)
            .Select(c => c.DescendantID)
            .Distinct();
        NodeClosure subtreeRootToTreeRootClosure = 
            await context.NodesClosureTable
                .AsNoTracking()
                .SingleAsync(c =>
                    c.AncestorID == null &&
                    c.DescendantID == nodeID
                );
        IQueryable<NodeClosure> oldSubtreeClosures = context.NodesClosureTable
            .Where(c => subtreeNodesIDs.Contains(c.DescendantID));
        IQueryable<NodeClosure> oldClosuresUnderSubtree = oldSubtreeClosures
            .Where(c =>
                c.AncestorID == null ||
                !subtreeNodesIDs.Contains((int)c.AncestorID)
            );
        var subtreeNodesAboveSubtreeRoot =
            oldSubtreeClosures
                .Where(c => 
                    (c.AncestorID == c.DescendantID &&
                    subtreeNodesIDs.Contains((int)c.AncestorID)) &&
                    c.DescendantID != nodeID
                )
                .Select(c => new { c.DescendantID, c.Distance });
        List<NodeClosure> newClosuresUnderSubtreeForSubtreeRoot = 
            await __CreateNewNodeClosures(
                context,
                newParentIDOrNull,
                nodeID);
        newClosuresUnderSubtreeForSubtreeRoot.Remove(
            newClosuresUnderSubtreeForSubtreeRoot
                .Single(c => c.AncestorID == c.DescendantID)
        );
        IEnumerable<NodeClosure> newClosuresUnderSubtreeForNodesAboveSubtreeRoot =
            await subtreeNodesAboveSubtreeRoot
                .Select(d => newClosuresUnderSubtreeForSubtreeRoot
                    .Select(c =>
                        new NodeClosure {
                            ID = 0,
                            AncestorID = c.AncestorID,
                            DescendantID = d.DescendantID,
                            Distance = c.Distance + d.Distance + 1
                        }
                    )
                ).SelectMany(cl => cl)
                .ToArrayAsync();
        NtwkNode subtreeRootNode = await context.Nodes.FindAsync(nodeID);

        subtreeRootNode.ParentID = newParentIDOrNull;
        context.NodesClosureTable.RemoveRange(oldClosuresUnderSubtree);
        context.NodesClosureTable.AddRange(newClosuresUnderSubtreeForSubtreeRoot);
        context.NodesClosureTable.AddRange(newClosuresUnderSubtreeForNodesAboveSubtreeRoot);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Profile>> GetAllProfiles() {
        return await context.Profiles
            .AsNoTracking()
            .OrderBy(p => p.ID)
            .ToListAsync();
    }
    
    public async Task<Model.IntermediateModel.AllRawNodesData> GetAllNodesData() {
        List<Model.ViewModel.CWSData> servicesData = await context.WebServices.AsNoTracking()
            .Select(ws => new Model.ViewModel.CWSData() {
                ID = ws.ID,
                Name = ws.Name
            })
            .OrderBy(wsd => wsd.ID)
            .ToListAsync();
        List<IEnumerable<Model.IntermediateModel.RawNodeData>> nodes = (
            await context.NodesClosureTable.AsNoTracking()
                .Where(cl => cl.AncestorID == null) //unique nodes
                .Include(cl => cl.Descendant)
                    .ThenInclude(n => n.Tags)
                .Include(cl => cl.Descendant)
                    .ThenInclude(n => n.CustomWebServices)
                        .ThenInclude(wsb => wsb.Service)
                .ToListAsync()
        )
            .GroupBy(cl => cl.Distance) //by depth layer
            .OrderBy(group => group.Key)
            .Select(group => group.Select(cl => cl.Descendant)
            )
            .Select(group => group
                .Select(n => new Model.IntermediateModel.RawNodeData() {
                    Node = n,
                    TagsIDs = n.Tags.Select(ta => ta.TagID).ToArray(),
                    BoundWebServicesIDs = n.CustomWebServices
                        .Select(wsb => wsb.ServiceID)
                        .OrderBy(id => id)
                        .ToArray()
                })
            ).ToList();

        return new Model.IntermediateModel.AllRawNodesData() {
            WebServicesData = servicesData,
            NodesData = nodes
        };
    }

    public async Task<uint> GetNodeIP(int nodeID) {
        return (
            await context.Nodes.AsNoTracking()
                .SingleAsync(n => n.ID == nodeID)
        ).ip;
    }
    
    public async Task<IEnumerable<int>> GetTaggedNodesIDs(int tagID) {
        return await context.TagAttachments
            .AsNoTracking()
            .Where(ta => ta.TagID == tagID)
            .Select(ta => ta.NodeID)
            .Distinct()
            .ToListAsync();
    }

    private static async Task<Model.ViewModel.TagFilterData> GetProfileTagFilterData(
        NtwkDBContext context,
        int profileID,
        ProfileSelectedTagFlags flag
    ) {
        var data = await context.ProfilesTagSelection.AsNoTracking()
            .Where(pst => pst.BindedProfileID == profileID &&
                        flag == (pst.Flags & flag))
            .Select(pst => new {
                tagID = pst.TagID,
                nodesIDs = pst.Tag.Attachments.Select(a => a.Node.ID)
            })
            .ToListAsync();
        return new Model.ViewModel.TagFilterData() {
            TagsIDs = data.Select(d => d.tagID),
            NodesIDs = data.SelectMany(d => d.nodesIDs).Distinct()
        };
    }

    public async Task<Model.ViewModel.TagFilterData> GetProfileViewTagFilterData(
        int profileID
    ) {
        return await GetProfileTagFilterData(
            context, profileID, ProfileSelectedTagFlags.NodesListView);
    }

    public async Task<Model.ViewModel.TagFilterData> GetProfileMonitorTagFilterData(
        int profileID
    ) {
        return await GetProfileTagFilterData(
            context, profileID, ProfileSelectedTagFlags.Monitor);
    }
    
    public async Task<IEnumerable<NodeTag>> GetAllTags() {
        return await context.Tags
            .AsNoTracking()
            .OrderBy(t => t.ID)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<CustomWebService>> GetAllCWS() {
        return await context.WebServices
            .AsNoTracking()
            .OrderBy(cvs => cvs.ID)
            .ToListAsync();
    }

    public  async Task<string> GetCWSBoundingString(int nodeID, int cwsID) {
        CustomWSBinding binding = await context.WebServiceBindings.AsNoTracking()
            .Where(wsb => wsb.NodeID == nodeID && wsb.ServiceID == cwsID)
            .Include(wsb => wsb.Service)
            .Include(wsb => wsb.Node)
            .SingleOrDefaultAsync();
        if(binding == null) {
            return null;
        }
        string rawServiceString = binding.Service.ServiceStr;
        string ip = new System.Net.IPAddress(binding.Node.ip).ToString();
        KeyValuePair<string, string>[] paramSwap = {
            new KeyValuePair<string, string>("{param1}", binding.Param1),
            new KeyValuePair<string, string>("{param2}", binding.Param2),
            new KeyValuePair<string, string>("{param3}", binding.Param3),
            new KeyValuePair<string, string>("{node_ip}", ip),
        };
        StringBuilder buildServiceStr = new StringBuilder(rawServiceString);
        foreach(KeyValuePair<string, string> swapper in paramSwap) {
            buildServiceStr.Replace(swapper.Key, swapper.Value);
        }
        return buildServiceStr.ToString();
    }


    public async Task<Profile> CreateProfile(Profile profile) {
        Profile newProfile = new Profile {
            ID = 0,
            Name = profile.Name,
            MonitoringStartHour = profile.MonitoringStartHour,
            MonitoringSessionDuration = profile.MonitoringSessionDuration,
            StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
            DepthMonitoring = profile.DepthMonitoring,
            MonitorInterval = profile.MonitorInterval
        };

        context.Profiles.Add(newProfile);
        await context.SaveChangesAsync();
        context.Entry(newProfile).State = EntityState.Detached;
        return newProfile;
    }
    
    public async Task<NtwkNode> CreateNodeOnRoot(NtwkNode node) {
        NtwkNode newNode = new NtwkNode {
            ID = 0,
            Parent = null,
            ParentPort = node.ParentPort,
            Name = node.Name,
            ip = node.ip,
            OpenTelnet = node.OpenTelnet,
            OpenSSH = node.OpenSSH,
            OpenPing = node.OpenPing
        };
        context.Nodes.Add(newNode);
        context.NodesClosureTable.AddRange(
            await __CreateNewNodeClosures(context, null, newNode.ID)
        );
        await context.SaveChangesAsync();
        context.Entry(newNode).State = EntityState.Detached;
        return newNode;
    }

    public async Task<NtwkNode> CreateNodeWithParent(NtwkNode node, int parentID) {
        NtwkNode newNode = new NtwkNode {
            ID = 0,
            ParentID = parentID,
            ParentPort = node.ParentPort,
            Name = node.Name,
            ip = node.ip,
            OpenTelnet = node.OpenTelnet,
            OpenSSH = node.OpenSSH,
            OpenPing = node.OpenPing
        };
        context.Nodes.Add(newNode);
        context.NodesClosureTable.AddRange(
            await __CreateNewNodeClosures(context, parentID, newNode.ID)
        );
        await context.SaveChangesAsync();
        context.Entry(newNode).State = EntityState.Detached;
        newNode.Parent = null;
        return newNode;
    }
    
    public async Task<NodeTag> CreateTag(NodeTag tag) {
        NodeTag newTag = new NodeTag {
            ID = 0,
            Name = tag.Name
        };
        context.Tags.Add(newTag);
        await context.SaveChangesAsync();
        context.Entry(newTag).State = EntityState.Detached;
        return newTag;
    }

    public async Task CreateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        CustomWSBinding binding = new CustomWSBinding {
            ID = 0,
            ServiceID = cwsID,
            NodeID = nodeID,
            Param1 = param1,
            Param2 = param2,
            Param3 = param3,
        };
        context.WebServiceBindings.Add(binding);
        await context.SaveChangesAsync();
    }

    public async Task SetNodeTags(int nodeID, IEnumerable<int> tagIDs) {
        IEnumerable<int> alreadyAttachedTagsIDs = await context.TagAttachments
            .Where(a => a.NodeID == nodeID)
            .Select(a => a.TagID)
            .ToArrayAsync();
        IEnumerable<int> idsOfTagsToRemove = alreadyAttachedTagsIDs
            .Except(tagIDs);
        IEnumerable<TagAttachment> tagAttachmentsToRemove = await context.TagAttachments
            .Where(a => idsOfTagsToRemove.Contains(a.TagID))
            .ToArrayAsync();
        IEnumerable<TagAttachment> newTagAttachments = tagIDs
            .Except(alreadyAttachedTagsIDs)
            .Select(id => new TagAttachment {
                ID = 0,
                TagID = id,
                NodeID = nodeID,
            });
        
        context.TagAttachments.RemoveRange(tagAttachmentsToRemove);
        context.TagAttachments.AddRange(newTagAttachments);
        await context.SaveChangesAsync();
    }

    private static async Task __SetProfileTagSelection(
        NtwkDBContext context,
        int profileID,
        IEnumerable<int> tagIDs,
        ProfileSelectedTagFlags flag
    ) {
        IEnumerable<ProfileSelectedTag> currentSelection =
            await context.ProfilesTagSelection
                .Where(pts => pts.BindedProfileID == profileID)
                .ToListAsync();
        var currentWithFlag = currentSelection
            .Where(pts => flag == (pts.Flags & flag))
            .Select(pts => new {
                toStay = tagIDs.Contains(pts.TagID),
                pts
            })
            .GroupBy(t => t.toStay)
            .Select(collection => new {
                collection.First().toStay,
                col = collection.Select(t => t.pts)
            });
        IEnumerable<int> toStay = currentWithFlag
            .SingleOrDefault(t => t.toStay)
            ?.col?.Select(pst => pst.TagID).ToList();
        IEnumerable<ProfileSelectedTag> toReset = currentWithFlag
            .SingleOrDefault(t => !t.toStay)
            ?.col;
        toStay = toStay ?? (new List<int>());
        toReset = toReset ?? (new List<ProfileSelectedTag>());
        IEnumerable<ProfileSelectedTag> currentWithoutFlag_ToSet = 
            currentSelection
                .Where(pts => flag != (pts.Flags & flag))
                .Where(pts => tagIDs.Contains(pts.TagID))
                .ToList();
        foreach(ProfileSelectedTag pst in currentWithoutFlag_ToSet) {
            pst.Flags = pst.Flags | flag;
        }
        List<ProfileSelectedTag> toRemove = new List<ProfileSelectedTag>();
        foreach(ProfileSelectedTag pst in toReset) {
            if(ProfileSelectedTagFlags.None == (pst.Flags ^ flag))
                toRemove.Add(pst);
            else
                pst.Flags = pst.Flags ^ flag;
        }
        context.RemoveRange(toRemove);

        if((toStay.Count() + currentWithoutFlag_ToSet.Count()) < tagIDs.Count()) {
            IEnumerable<int> alreadySetTags = new [] {
                toStay,
                currentWithoutFlag_ToSet.Select(pst => pst.TagID),
            }
            .SelectMany(t => t);
            IEnumerable<int> notSetTags = tagIDs.Except(alreadySetTags);
            IEnumerable<ProfileSelectedTag> newSelections = notSetTags
                .Select(tagID => new ProfileSelectedTag {
                    ID = 0,
                    Flags = flag,
                    BindedProfileID = profileID,
                    TagID = tagID
                });
            context.ProfilesTagSelection.AddRange(newSelections);
        }
        
        await context.SaveChangesAsync();
    }

    private static async Task __SetOneSelectionToAnother(
        NtwkDBContext context,
        int profileID,
        ProfileSelectedTagFlags from,
        ProfileSelectedTagFlags to
    ) {
        IEnumerable<ProfileSelectedTag> fromAndToSelections =
            await context.ProfilesTagSelection
                .Where(pst => from == (pst.Flags & from) ||
                    to == (pst.Flags & to))
                .ToListAsync();
        IEnumerable<ProfileSelectedTag> toSet = fromAndToSelections
            .Where(pst => from == (pst.Flags & from) &&
                to != (pst.Flags & to));
        IEnumerable<ProfileSelectedTag> toRemove = fromAndToSelections
            .Where(pst => ProfileSelectedTagFlags.None == (pst.Flags ^ to));
        foreach(ProfileSelectedTag pst in toSet) {
            pst.Flags = pst.Flags | to;
        }
        context.ProfilesTagSelection.RemoveRange(toRemove);
        await context.SaveChangesAsync();
    }

    public async Task SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        await __SetProfileTagSelection(context, profileID, tagIDs,
            ProfileSelectedTagFlags.NodesListView);
    }

    
    public async Task SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID) {
        await __SetOneSelectionToAnother(context, profileID,
            ProfileSelectedTagFlags.Monitor,
            ProfileSelectedTagFlags.NodesListView
        );
    }

    public async Task SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        await __SetProfileTagSelection(context, profileID, tagIDs,
            ProfileSelectedTagFlags.Monitor);
    }

    public async Task SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        await __SetOneSelectionToAnother(context, profileID,
            ProfileSelectedTagFlags.NodesListView,
            ProfileSelectedTagFlags.Monitor
        );
    }
    
    public async Task<CustomWebService> CreateCustomWebService(CustomWebService cws) {
        CustomWebService newCWS = new CustomWebService {
            ID = 0,
            Name = cws.Name,
            ServiceStr = cws.ServiceStr,
            Parametr1Name = cws.Parametr1Name,
            Parametr2Name = cws.Parametr2Name,
            Parametr3Name = cws.Parametr3Name,
        };
        context.WebServices.Add(newCWS);
        await context.SaveChangesAsync();
        context.Entry(newCWS).State = EntityState.Detached;
        return newCWS;
    }
    
    public async Task UpdateProfile(Profile newProfileData) {
        Profile profile = await context.Profiles.FindAsync(newProfileData.ID);
        profile.Name = newProfileData.Name;
        profile.MonitoringStartHour = newProfileData.MonitoringStartHour;
        profile.MonitoringSessionDuration = newProfileData.MonitoringSessionDuration;
        profile.StartMonitoringOnLaunch = newProfileData.StartMonitoringOnLaunch;
        profile.DepthMonitoring = newProfileData.DepthMonitoring;
        profile.MonitorInterval = newProfileData.MonitorInterval;
        await context.SaveChangesAsync();
    }

    public async Task UpdateTag(NodeTag newNodeTagData) {
        NodeTag tag = await context.Tags.FindAsync(newNodeTagData.ID);
        tag.Name = newNodeTagData.Name;
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateNode(NtwkNode newNodeData) {
        NtwkNode node = await context.Nodes.FindAsync(newNodeData.ID);
        node.Name = newNodeData.Name;
        node.ParentPort = newNodeData.ParentPort;
        node.ip = newNodeData.ip;
        node.OpenPing = newNodeData.OpenPing;
        node.OpenSSH = newNodeData.OpenSSH;
        node.OpenTelnet = newNodeData.OpenTelnet;
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateCustomWebService(CustomWebService cws) {
        CustomWebService cwsToUpdate = 
            await context.WebServices.FindAsync(cws.ID);
        cwsToUpdate.Name = cws.Name;
        cwsToUpdate.ServiceStr = cws.ServiceStr;
        cwsToUpdate.Parametr1Name = cws.Parametr1Name;
        cwsToUpdate.Parametr2Name = cws.Parametr2Name;
        cwsToUpdate.Parametr3Name = cws.Parametr3Name;
        await context.SaveChangesAsync();
    }

    public async Task UpdateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        var binding = await context.WebServiceBindings
            .SingleAsync(wsb => wsb.NodeID == nodeID && wsb.ServiceID == cwsID);
        binding.Param1 = param1;
        binding.Param2 = param2;
        binding.Param3 = param3;
        await context.SaveChangesAsync();
    }
    

    public async Task<Profile> RemoveProfile(int profileID) {
        Profile profile = await context.Profiles
            .Include(p => p.MonitoringSessions)
                .ThenInclude(s => s.Pulses)
                    .ThenInclude(p => p.Messages)
            .Include(p => p.FilterTagSelection)
            .SingleAsync(p => p.ID == profileID);
        IEnumerable<ProfileSelectedTag> viewTagsSelection = profile.FilterTagSelection;
        IEnumerable<MonitoringSession> sessions = profile.MonitoringSessions;
        IEnumerable<MonitoringPulseResult> pulses = sessions.SelectMany(s => s.Pulses);
        IEnumerable<MonitoringMessage> messages = pulses.SelectMany(p => p.Messages);
        context.ProfilesTagSelection.RemoveRange(viewTagsSelection);
        context.MonitoringMessages.RemoveRange(messages);
        context.MonitoringPulses.RemoveRange(pulses);
        context.MonitoringSessions.RemoveRange(sessions);
        profile.MonitoringSessions = null;
        context.Profiles.Remove(profile);
        await context.SaveChangesAsync();
        return profile;
    }
    
    public async Task<NtwkNode> RemoveNode(int nodeID) {
        NtwkNode node = await context.Nodes
            .Include(n => n.Children)
            .SingleAsync(n => n.ID == nodeID);
        if(node.Children.Count != 0) {
            throw new InvalidOperationException();
        }
        context.Nodes.Remove(node);
        await context.SaveChangesAsync();
        return node;
    }
    
    public async Task<NodeTag> RemoveTag(int tagID) {
        NodeTag tag = await context.Tags
            .Include(t => t.Attachments)
            .Include(t => t.ProfilesFilterSelections)
            .SingleAsync(t => t.ID == tagID);
        context.RemoveRange(tag.Attachments);
        context.RemoveRange(tag.ProfilesFilterSelections);
        tag.Attachments = null;
        tag.ProfilesFilterSelections = null;
        context.Remove(tag);
        await context.SaveChangesAsync();
        return tag;
    }
    
    public async Task<CustomWebService> RemoveCustomWebService(int cwsID) {
        CustomWebService service = await context.WebServices
            .Include(ws => ws.Bindings)
            .SingleAsync(ws => ws.ID == cwsID);
        context.RemoveRange(service.Bindings);
        service.Bindings = null;
        context.Remove(service);
        await context.SaveChangesAsync();
        return service;
    }

    public async Task RemoveWebServiceBinding(
        int nodeID,
        int cwsID
    ) {
        var binding = await context.WebServiceBindings
            .SingleAsync(wsb => wsb.NodeID == nodeID && wsb.ServiceID == cwsID);
        context.WebServiceBindings.Remove(binding);
        await context.SaveChangesAsync();
    }
}

}