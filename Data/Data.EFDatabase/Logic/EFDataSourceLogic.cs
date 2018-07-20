//I thought this kind of separation of logic will help with testing.
//I'm not sure it did in this case.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Data.Model.EFDbModel;
using Data.Model.Enums;

namespace Data.EFDatabase.Logic {

public static class EFDataSourceLogic {

    static double JSDateTimeNow() {
        return DateTime.UtcNow
               .Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc))
               .TotalMilliseconds;
    }

    public static async Task<MonitoringSession> GetNewSession_Logic(
        NtwkDBContext context,
        int profileID,
        IEnumerable<int> monitoredTagsIDList
    ) {
        int monitoredNodesNum = await context.Tags
            .Where(t => monitoredTagsIDList.Contains(t.ID))
            .Include(t => t.Attachments)
            .ThenInclude(ta => ta.Node)
            .SelectMany(t => t.Attachments.Select(ta => ta.Node.ID))
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

    public static async Task<MonitoringPulseResult> SavePulseResult_Logic(
        NtwkDBContext context,
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

    //TODO: somehow move this logic to asp.net app shutdown event
    public static async Task ClearEmptySessions_Logic(NtwkDBContext context) {
        IQueryable<MonitoringSession> EmptySessions = context.MonitoringSessions
            .Where(s => s.LastPulseTime == 0);
        context.MonitoringSessions.RemoveRange(EmptySessions);
        await context.SaveChangesAsync();
    }

    public static async Task<IEnumerable<MonitoringSession>> GetSessionsForProfile_Logic(
        NtwkDBContext context,
        int profileID
    ) {
        return await context.MonitoringSessions
            .AsNoTracking()
            .Where(s => s.CreatedByProfileID == profileID)
            .OrderByDescending(s => s.CreationTime)
            .ToListAsync();
    }
    
    public static async Task<IEnumerable<MonitoringPulseResult>> GetSessionReport_Logic(
        NtwkDBContext context,
        int monitoringSessionID
    ) {
        return (await context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.Pulses)
            .SingleAsync(s => s.ID == monitoringSessionID))
            .Pulses;
    }
    

    public static async Task<IEnumerable<Profile>> GetAllProfiles_Logic(NtwkDBContext context) {
        return await context.Profiles
            .AsNoTracking()
            .OrderByDescending(p => p.ID)
            .ToListAsync();
    }

    public static async Task<IEnumerable<NtwkNode>> GetAllNodes_Logic(
        NtwkDBContext context
    ) {
        return await context.Nodes.AsNoTracking().ToListAsync();
    }
    
    public static async Task<IEnumerable<int>> GetTaggedNodesIDs_Logic(
        NtwkDBContext context,
        int tagID
    ) {
        return await context.TagAttachments
            .AsNoTracking()
            .Where(ta => ta.TagID == tagID)
            .Select(ta => ta.NodeID)
            .Distinct()
            .ToListAsync();
    }

    static async Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfile(
        NtwkDBContext context,
        int profileID,
        ProfileSelectedTagFlags flag
    ) {
        return await context.ProfilesTagSelection.AsNoTracking()
            .Where(pst => pst.BindedProfileID == profileID &&
                        flag == (pst.Flags & flag))
            .Include(pst => pst.Tag)
                .ThenInclude(t => t.Attachments)
            .SelectMany(pst => pst.Tag.Attachments.Select(a => a.Node.ID))
            .Distinct()
            .ToListAsync();
    }

    public static async Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfileView_Logic(
        NtwkDBContext context,
        int profileID
    ) {
        return await GetIDsOfNodesBySelectedTagsInProfile(
            context, profileID, ProfileSelectedTagFlags.NodesListView);
    }

    public static async Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfileMonitor_Logic(
        NtwkDBContext context,
        int profileID
    ) {
        return await GetIDsOfNodesBySelectedTagsInProfile(
            context, profileID, ProfileSelectedTagFlags.Monitor);
    }
    
    public static async Task<IEnumerable<NodeTag>> GetAllTags_Logic(NtwkDBContext context) {
        return await context.Tags
            .AsNoTracking()
            .OrderByDescending(t => t.ID)
            .ToListAsync();
    }
    
    public static async Task<IEnumerable<CustomWebService>> GetAllCVS_Logic(NtwkDBContext context) {
        return await context.WebServices
            .AsNoTracking()
            .OrderByDescending(cvs => cvs.ID)
            .ToListAsync();
    }

    public static async Task<Profile> CreateProfile_Logic(
        NtwkDBContext context,
        Profile profile
    ) {
        Profile newProfile = new Profile {
            ID = 0,
            Name = profile.Name,
            MonitoringAlarmEmail = profile.MonitoringAlarmEmail,
            SendMonitoringAlarm = profile.SendMonitoringAlarm,
            MonitoringStartHour = profile.MonitoringStartHour,
            MonitoringEndHour = profile.MonitoringEndHour,
            StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
            DepthMonitoring = profile.DepthMonitoring,
            MonitorInterval = profile.MonitorInterval
        };

        context.Profiles.Add(newProfile);
        await context.SaveChangesAsync();
        context.Entry(newProfile).State = EntityState.Detached;
        return newProfile;
    }
    
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

    public static async Task<NtwkNode> CreateNodeOnRoot_Logic(
        NtwkDBContext context,
        NtwkNode node
    ) {
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

    public static async Task<NtwkNode> CreateNodeWithParent_Logic(
        NtwkDBContext context,
        NtwkNode node,
        int parentID
    ) {
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

    public static async Task<bool> HasChildren_Logic(
        NtwkDBContext context,
        int nodeID
    ) {
        int nodesWithThisAncestor = await context.NodesClosureTable
            .Where(c => c.AncestorID == nodeID)
            .CountAsync();
        return nodesWithThisAncestor != 1; //only self ref closure
    }

    public static async Task<bool> CheckIfProfileExists_Logic(
        NtwkDBContext context,
        int profileID
    ) {
        return await context.Profiles.AnyAsync(n => n.ID == profileID);
    }

    public static async Task<bool> CheckIfTagExists_Logic(
        NtwkDBContext context,
        int tagID
    ) {
        return await context.Tags.AnyAsync(n => n.ID == tagID);
    }

    public static async Task<bool> CheckIfTagsExist_Logic(
        NtwkDBContext context,
        IEnumerable<int> tagsIDs
    ) {
        return tagsIDs.Count() == 
            await context.Tags.Where(t => tagsIDs.Contains(t.ID)).CountAsync();
    }

    public static async Task<bool> CheckIfNodeExists_Logic(
        NtwkDBContext context,
        int nodeID
    ) {
        return await context.Nodes.AnyAsync(n => n.ID == nodeID);
    }

    static async Task<bool> __CheckIfNameExists<T>(
        IQueryable<T> collection,
        Func<T, string> namePred,
        string name
    ) {
        return await collection
            .Where(e => namePred(e) == name)
            .AnyAsync();
    }

    public static async Task<bool> CheckIfTagNameExists_Logic(
        NtwkDBContext context,
        string name
    ) {
        return await __CheckIfNameExists(
            context.Tags.AsNoTracking(),
            t => t.Name,
            name
        );
    }

    public static async Task<bool> CheckIfNodeNameExists_Logic(
        NtwkDBContext context,
        string name
    ) {
        return await __CheckIfNameExists(
            context.Nodes.AsNoTracking(),
            t => t.Name,
            name
        );
    }

    public static async Task<bool> CheckIfCWSNameExists_Logic(
        NtwkDBContext context,
        string name
    ) {
        return await __CheckIfNameExists(
            context.WebServices.AsNoTracking(),
            t => t.ServiceName,
            name
        );
    }
    
    public static async Task<NodeTag> CreateTag_Logic(
        NtwkDBContext context,
        NodeTag tag
    ) {
        NodeTag newTag = new NodeTag {
            ID = 0,
            Name = tag.Name
        };
        context.Tags.Add(newTag);
        await context.SaveChangesAsync();
        context.Entry(newTag).State = EntityState.Detached;
        return newTag;
    }

    public static async Task SetNodeTags_Logic(
        NtwkDBContext context,
        int nodeID,
        IEnumerable<int> tagIDs
    ) {
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

    public static async Task CreateWebServiceBinding_Logic(
        NtwkDBContext context,
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

    static async Task __SetProfileTagSelection(
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
        toStay = (toStay == null)?(new List<int>()):(toStay);
        toReset = (toReset == null)?(new List<ProfileSelectedTag>()):(toReset);
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

    public static async Task SetProfileViewTagsSelection_Logic(
        NtwkDBContext context,
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        await __SetProfileTagSelection(context, profileID, tagIDs,
            ProfileSelectedTagFlags.NodesListView);
    }

    public static async Task SetProfileMonitorTagsSelection_Logic(
        NtwkDBContext context,
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        await __SetProfileTagSelection(context, profileID, tagIDs,
            ProfileSelectedTagFlags.Monitor);
    }

    static async Task __SetOneSelectionToAnother(
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

    public static async Task SetProfileViewTagsSelectionToProfileMonitorTagsSelection_Logic(
        NtwkDBContext context,
        int profileID
    ) {
        await __SetOneSelectionToAnother(context, profileID,
            ProfileSelectedTagFlags.Monitor,
            ProfileSelectedTagFlags.NodesListView
        );
    }

    public static async Task SetProfileMonitorTagsSelectionToProfileViewTagsSelection_Logic(
        NtwkDBContext context,
        int profileID
    ) {
        await __SetOneSelectionToAnother(context, profileID,
            ProfileSelectedTagFlags.NodesListView,
            ProfileSelectedTagFlags.Monitor
        );
    }
    
    public static async Task<CustomWebService> CreateCustomWebService_Logic(
        NtwkDBContext context,
        CustomWebService cws
    ) {
        CustomWebService newCWS = new CustomWebService {
            ID = 0,
            ServiceName = cws.ServiceName,
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
    

    public static async Task UpdateProfile_Logic(
        NtwkDBContext context,
        Profile newProfileData
    ) {
        Profile profile = await context.Profiles.FindAsync(newProfileData.ID);
        profile.Name = newProfileData.Name;
        profile.SendMonitoringAlarm = newProfileData.SendMonitoringAlarm;
        profile.MonitoringAlarmEmail = newProfileData.MonitoringAlarmEmail;
        profile.MonitoringStartHour = newProfileData.MonitoringStartHour;
        profile.MonitoringEndHour = newProfileData.MonitoringEndHour;
        profile.StartMonitoringOnLaunch = newProfileData.StartMonitoringOnLaunch;
        profile.DepthMonitoring = newProfileData.DepthMonitoring;
        profile.MonitorInterval = newProfileData.MonitorInterval;
        await context.SaveChangesAsync();
    }
    
    public static async Task UpdateNode_Logic(
        NtwkDBContext context,
        NtwkNode newNodeData
    ) {
        NtwkNode node = await context.Nodes.FindAsync(newNodeData.ID);
        node.Name = newNodeData.Name;
        node.ParentPort = newNodeData.ParentPort;
        node.ip = newNodeData.ip;
        node.OpenPing = newNodeData.OpenPing;
        node.OpenSSH = newNodeData.OpenSSH;
        node.OpenTelnet = newNodeData.OpenTelnet;
        await context.SaveChangesAsync();
    }

    //IMPORTANT TO PREVENT CICLES
    public static async Task<bool> CheckIfNodeInSubtree_Logic(
        NtwkDBContext context,
        int nodeID,
        int subtreeRootNodeID
    ) {
        return await context.NodesClosureTable
            .AnyAsync(c => 
                c.AncestorID == subtreeRootNodeID &&
                c.DescendantID == nodeID
            );
    }

    //Should be checked by higher level
    //services for newParent not being part of node's subtree.
    //Sidenote. Worst implementation ever
    public static async Task MoveNodesSubtree_Logic(
        NtwkDBContext context,
        int nodeID,
        int newParentID
    ) {
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
            await __CreateNewNodeClosures(context, newParentID, nodeID);
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

        subtreeRootNode.ParentID = newParentID;
        context.NodesClosureTable.RemoveRange(oldClosuresUnderSubtree);
        context.NodesClosureTable.AddRange(newClosuresUnderSubtreeForSubtreeRoot);
        context.NodesClosureTable.AddRange(newClosuresUnderSubtreeForNodesAboveSubtreeRoot);
        await context.SaveChangesAsync();
    }
    
    public static async Task UpdateCustomWebService_Logic(
        NtwkDBContext context,
        CustomWebService cws
    ) {
        CustomWebService cwsToUpdate = 
            await context.WebServices.FindAsync(cws.ID);
        cwsToUpdate.ServiceName = cws.ServiceName;
        cwsToUpdate.ServiceStr = cws.ServiceStr;
        cwsToUpdate.Parametr1Name = cws.Parametr1Name;
        cwsToUpdate.Parametr2Name = cws.Parametr2Name;
        cwsToUpdate.Parametr3Name = cws.Parametr3Name;
        await context.SaveChangesAsync();
    }

    public static async Task UpdateWebServiceBinding_Logic(
        NtwkDBContext context,
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
    

    public static async Task<Profile> RemoveProfile_Logic(
        NtwkDBContext context,
        int profileID
    ) {
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
    
    //Has to be checked for not having any child nodes
    //Or will throw staff at the caller
    public static async Task<NtwkNode> RemoveNode_Logic(
        NtwkDBContext context,
        int nodeID
    ) {
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
    
    public static async Task<NodeTag> RemoveTag_Logic(
        NtwkDBContext context,
        int tagID
    ) {
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
    
    public static async Task<CustomWebService> RemoveCustomWebService_Logic(
        NtwkDBContext context,
        int cwsID
    ) {
        CustomWebService service = await context.WebServices
            .Include(ws => ws.Bindings)
            .SingleAsync(ws => ws.ID == cwsID);
        context.RemoveRange(service.Bindings);
        service.Bindings = null;
        context.Remove(service);
        await context.SaveChangesAsync();
        return service;
    }

    public static async Task RemoveWebServiceBinding_Logic(
        NtwkDBContext context,
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