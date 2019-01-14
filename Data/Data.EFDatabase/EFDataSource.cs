using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.Abstract.DbInteraction;

namespace Data.EFDatabase
{
    public class EfDataSource : IEfDbDataSource
    {
        private readonly NtwkDBContext _context;

        public EfDataSource(NtwkDBContext context)
        {
            _context = context;
        }

        private static double JsDateTimeNow()
        {
            return DateTime.UtcNow
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;
        }

        public async Task<MonitoringSession> GetNewSession(
            int profileId
        )
        {
            bool hasMonitorTagFilter = await _context.ProfilesTagSelection
                .AnyAsync(pts => pts.BindedProfileID == profileId &&
                                 ProfileSelectedTagFlags.Monitor ==
                                 (pts.Flags & ProfileSelectedTagFlags.Monitor));
            int monitoredNodesNum = hasMonitorTagFilter
                ? await _context.ProfilesTagSelection.AsNoTracking()
                    .Where(pts => pts.BindedProfileID == profileId &&
                                  ProfileSelectedTagFlags.Monitor ==
                                  (pts.Flags & ProfileSelectedTagFlags.Monitor))
                    .Include(pts => pts.Tag)
                    .ThenInclude(t => t.Attachments)
                    .ThenInclude(ta => ta.Node)
                    .SelectMany(pts => pts.Tag.Attachments.Select(ta => ta.Node))
                    .Where(n => n.OpenPing)
                    .Select(n => n.ID)
                    .Distinct()
                    .CountAsync()
                : await _context.Nodes.CountAsync(n => n.OpenPing);
            MonitoringSession newSession = new MonitoringSession
            {
                ID = 0,
                CreatedByProfileID = profileId,
                ParticipatingNodesNum = monitoredNodesNum,
                CreationTime = JsDateTimeNow(),
                LastPulseTime = 0,
            };
            _context.MonitoringSessions.Add(newSession);
            await _context.SaveChangesAsync();
            _context.Entry(newSession).State = EntityState.Detached;
            return newSession;
        }

        public async Task<MonitoringPulseResult> SavePulseResult(
            int sessionId,
            MonitoringPulseResult pulseResult,
            IEnumerable<MonitoringMessage> messages
        )
        {
            pulseResult.CreationTime = JsDateTimeNow();
            MonitoringSession session = await _context.MonitoringSessions
                .Include(s => s.Pulses)
                .SingleAsync(s => s.ID == sessionId);
            //context.MonitoringPulses
            //    .Add(pulseResult);
            session.LastPulseTime = pulseResult.CreationTime;
            //await context.SaveChangesAsync();
            session.Pulses.Add(pulseResult);
            await _context.SaveChangesAsync();
            //foreach(MonitoringMessage message in messages) {
            //    context.MonitoringMessages.add
            //        .Add(message);
            //}
            //await context.SaveChangesAsync();
            MonitoringPulseResult pulseWithMessages = await _context.MonitoringPulses
                .Include(p => p.Messages)
                .SingleAsync(p => p.ID == pulseResult.ID);
            foreach (MonitoringMessage messageEntity in messages)
            {
                pulseWithMessages.Messages.Add(messageEntity);
            }

            await _context.SaveChangesAsync();
            _context.Entry(pulseWithMessages).State = EntityState.Detached;
            return pulseWithMessages;
        }

        public async Task ClearEmptySessions()
        {
            double TOLERANCE = 0.0001;
            IQueryable<MonitoringSession> emptySessions = _context.MonitoringSessions
                .Where(s => Math.Abs(s.LastPulseTime) < TOLERANCE);
            _context.MonitoringSessions.RemoveRange(emptySessions);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MonitoringSession>> GetSessionsForProfile(int profileId)
        {
            return await _context.MonitoringSessions
                .AsNoTracking()
                .Where(s => s.CreatedByProfileID == profileId)
                .OrderByDescending(s => s.CreationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonitoringPulseResult>> GetSessionReport(int monitoringSessionId)
        {
            return (await _context.MonitoringSessions
                    .AsNoTracking()
                    .Include(s => s.Pulses)
                    .ThenInclude(p => p.Messages)
                    .SingleAsync(s => s.ID == monitoringSessionId))
                .Pulses;
        }


        public async Task<bool> HasChildren(int nodeId)
        {
            int nodesWithThisAncestor = await _context.NodesClosureTable
                .Where(c => c.AncestorID == nodeId)
                .CountAsync();
            return nodesWithThisAncestor != 1; //only self ref closure
        }

        public async Task<bool> CheckIfProfileExists(int profileId)
        {
            return await _context.Profiles.AnyAsync(n => n.ID == profileId);
        }

        private static int ParamNum(CustomWebService cws)
        {
            return cws.Parametr3Name != null
                ? 3
                : cws.Parametr2Name != null
                    ? 2
                    : cws.Parametr1Name != null
                        ? 1
                        : 0;
        }

        public async Task<int> GetCwsParamNumber(int cwsId)
        {
            CustomWebService cws = await _context.WebServices
                .SingleOrDefaultAsync(ws => ws.ID == cwsId);
            return cws == null ? (-1) : ParamNum(cws);
        }

        public async Task<bool> CheckIfSessionExists(int sessionId)
        {
            return await _context.MonitoringSessions.AnyAsync(n => n.ID == sessionId);
        }

        public async Task<bool> CheckIfCwsBindingExists(int cwsId, int nodeId)
        {
            return await _context.WebServiceBindings
                .AnyAsync(wsb =>
                    wsb.NodeID == nodeId &&
                    wsb.ServiceID == cwsId
                );
        }

        public async Task<bool> CheckIfTagExists(int tagId)
        {
            return await _context.Tags.AnyAsync(n => n.ID == tagId);
        }

        public async Task<bool> CheckIfTagsExist(IEnumerable<int> tagsIDs)
        {
            return await _context.Tags
                       .Where(t => tagsIDs.Contains(t.ID))
                       .CountAsync()
                   == tagsIDs.Count();
        }

        public async Task<bool> CheckIfNodeExists(int nodeId)
        {
            return await _context.Nodes.AnyAsync(n => n.ID == nodeId);
        }

        public async Task<bool> CheckIfTagNameExists(string name, int? exceptId)
        {
            return (
                       exceptId == null
                       || (await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.ID == exceptId))?.Name != name
                   ) && await _context.Tags.AsNoTracking()
                       .Where(e => e.Name == name)
                       .AnyAsync();
        }

        public async Task<bool> CheckIfNodeNameExists(string name, int? exceptId)
        {
            return (
                       exceptId == null
                       || (await _context.Nodes.AsNoTracking().FirstOrDefaultAsync(n => n.ID == exceptId))?.Name != name
                   ) && await _context.Nodes.AsNoTracking()
                       .Where(e => e.Name == name)
                       .AnyAsync();
        }

        public async Task<bool> CheckIfCwsNameExists(string name, int? exceptId)
        {
            return (
                       exceptId == null
                       || (await _context.WebServices.AsNoTracking().FirstOrDefaultAsync(ws => ws.ID == exceptId))
                       ?.Name != name
                   ) && await _context.WebServices.AsNoTracking()
                       .Where(e => e.Name == name)
                       .AnyAsync();
        }

        public async Task<bool> CheckIfProfileNameExists(string name, int? exceptId)
        {
            return (
                       exceptId == null
                       || (await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.ID == exceptId))?.Name !=
                       name
                   ) && await _context.Profiles.AsNoTracking()
                       .Where(e => e.Name == name)
                       .AnyAsync();
        }

        // building tree structure for node
        // public for testing purposes
        // should not be used outside this class
        public static async Task<List<NodeClosure>> __CreateNewNodeClosures(
            NtwkDBContext context,
            int? parentId,
            int nodeId
        )
        {
            List<NodeClosure> result;
            if (parentId != null)
            {
                result = await context.NodesClosureTable
                    .AsNoTracking()
                    .Where(c => c.DescendantID == parentId)
                    .Select(c => new NodeClosure
                    {
                        ID = 0,
                        AncestorID = c.AncestorID,
                        DescendantID = nodeId,
                        Distance = c.Distance + 1
                    })
                    .ToListAsync();
                result.Add(new NodeClosure
                {
                    ID = 0,
                    AncestorID = nodeId,
                    DescendantID = nodeId,
                    Distance = 0
                });
            }
            else
            {
                result = new[]
                {
                    new NodeClosure
                    {
                        ID = 0,
                        AncestorID = null,
                        DescendantID = nodeId,
                        Distance = 0
                    },
                    new NodeClosure
                    {
                        ID = 0,
                        AncestorID = nodeId,
                        DescendantID = nodeId,
                        Distance = 0
                    }
                }.ToList();
            }

            return result;
        }

        // CYCLE PREVENTION LOGIC
        public async Task<bool> CheckIfNodeInSubtree(
            int nodeId,
            int subtreeRootNodeId
        )
        {
            return await _context.NodesClosureTable
                .AnyAsync(c =>
                    c.AncestorID == subtreeRootNodeId &&
                    c.DescendantID == nodeId
                );
        }

        // Should be checked by higher level
        // services for newParent not being part of node's subtree.
        public async Task MoveNodesSubtree(int nodeId, int newParentId)
        {
            int? newParentIdOrNull = newParentId == 0 ? (int?) null : newParentId;
            IQueryable<int> subtreeNodesIDs = _context.NodesClosureTable
                .Where(c => c.AncestorID == nodeId)
                .Select(c => c.DescendantID)
                .Distinct();
            IQueryable<NodeClosure> oldSubtreeClosures = _context.NodesClosureTable
                .Where(c => subtreeNodesIDs.Contains(c.DescendantID));
            IQueryable<NodeClosure> oldClosuresUnderSubtree = oldSubtreeClosures
                .Where(c =>
                    c.AncestorID == null ||
                    !subtreeNodesIDs.Contains((int) c.AncestorID)
                );
            var subtreeNodesAboveSubtreeRoot =
                oldSubtreeClosures
                    .Where(c =>
                        c.AncestorID == nodeId && c.DescendantID != nodeId &&
                        subtreeNodesIDs.Contains(c.DescendantID)
                    )
                    .Select(c => new { c.DescendantID, c.Distance});
            List<NodeClosure> newClosuresUnderSubtreeForSubtreeRoot =
                await __CreateNewNodeClosures(
                    _context,
                    newParentIdOrNull,
                    nodeId);
            newClosuresUnderSubtreeForSubtreeRoot.Remove(
                newClosuresUnderSubtreeForSubtreeRoot
                    .Single(c => c.AncestorID == c.DescendantID)
            );
            IEnumerable<NodeClosure> newClosuresUnderSubtreeForNodesAboveSubtreeRoot =
                await subtreeNodesAboveSubtreeRoot
                    .Select(d => newClosuresUnderSubtreeForSubtreeRoot
                        .Select(c =>
                            new NodeClosure
                            {
                                ID = 0,
                                AncestorID = c.AncestorID,
                                DescendantID = d.DescendantID,
                                Distance = c.Distance + d.Distance
                            }
                        )
                    ).SelectMany(cl => cl)
                    .ToArrayAsync();
            NtwkNode subtreeRootNode = await _context.Nodes.FindAsync(nodeId);

            subtreeRootNode.ParentID = newParentIdOrNull;
            /* Debug output
            void Output(string title, NodeClosure[] arr)
            {
                Console.WriteLine(title);
                foreach (var t in arr)
                {
                    System.Diagnostics.Debug.WriteLine($"A-D-d ({t.AncestorID}, {t.DescendantID}, {t.Distance})");
                }
            }

            Output("Old closures under subtree", oldClosuresUnderSubtree.ToArray());
            Output("New closures under subtree for root", newClosuresUnderSubtreeForSubtreeRoot.ToArray());
            Output("New closures under subtree for other subtree nodes", newClosuresUnderSubtreeForNodesAboveSubtreeRoot.ToArray());
            */
            _context.NodesClosureTable.RemoveRange(oldClosuresUnderSubtree);
            _context.NodesClosureTable.AddRange(newClosuresUnderSubtreeForSubtreeRoot);
            _context.NodesClosureTable.AddRange(newClosuresUnderSubtreeForNodesAboveSubtreeRoot);
            //var oldSubtreeClosuresArray = oldSubtreeClosures.ToArray();
            await _context.SaveChangesAsync();
            //Output("All old subtree closures", oldSubtreeClosuresArray);
            //Output("All new subtree closures", oldSubtreeClosures.ToArray());
        }

        public async Task<IEnumerable<Profile>> GetAllProfiles()
        {
            return await _context.Profiles
                .AsNoTracking()
                .OrderBy(p => p.ID)
                .ToListAsync();
        }

        public async Task<Model.IntermediateModel.AllRawNodesData> GetAllNodesData()
        {
            List<Model.ViewModel.CwsData> servicesData = await _context.WebServices.AsNoTracking()
                .Select(ws => new Model.ViewModel.CwsData()
                {
                    Id = ws.ID,
                    Name = ws.Name
                })
                .OrderBy(wsd => wsd.Id)
                .ToListAsync();
            List<IEnumerable<Model.IntermediateModel.RawNodeData>> nodes = (
                    await _context.NodesClosureTable.AsNoTracking()
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
                    .Select(n => new Model.IntermediateModel.RawNodeData()
                    {
                        Node = n,
                        TagsIDs = n.Tags.Select(ta => ta.TagID).ToArray(),
                        BoundWebServicesIDs = n.CustomWebServices
                            .Select(wsb => wsb.ServiceID)
                            .OrderBy(id => id)
                            .ToArray()
                    })
                ).ToList();

            return new Model.IntermediateModel.AllRawNodesData()
            {
                WebServicesData = servicesData,
                NodesData = nodes
            };
        }

        public async Task<uint> GetNodeIp(int nodeId)
        {
            return (
                await _context.Nodes.AsNoTracking()
                    .SingleAsync(n => n.ID == nodeId)
            ).ip;
        }

        public async Task<IEnumerable<int>> GetTaggedNodesIDs(int tagId)
        {
            return await _context.TagAttachments
                .AsNoTracking()
                .Where(ta => ta.TagID == tagId)
                .Select(ta => ta.NodeID)
                .Distinct()
                .ToListAsync();
        }

        private static async Task<Model.ViewModel.TagFilterData> GetProfileTagFilterData(
            NtwkDBContext context,
            int profileId,
            ProfileSelectedTagFlags flag
        )
        {
            var data = await context.ProfilesTagSelection.AsNoTracking()
                .Where(pst => pst.BindedProfileID == profileId &&
                              flag == (pst.Flags & flag))
                .Select(pst => new
                {
                    tagID = pst.TagID,
                    nodesIDs = pst.Tag.Attachments.Select(a => a.Node.ID)
                })
                .ToListAsync();
            return new Model.ViewModel.TagFilterData()
            {
                TagsIDs = data.Select(d => d.tagID),
                NodesIDs = data.SelectMany(d => d.nodesIDs).Distinct()
            };
        }

        public async Task<Model.ViewModel.TagFilterData> GetProfileViewTagFilterData(
            int profileId
        )
        {
            return await GetProfileTagFilterData(
                _context, profileId, ProfileSelectedTagFlags.NodesListView);
        }

        public async Task<Model.ViewModel.TagFilterData> GetProfileMonitorTagFilterData(
            int profileId
        )
        {
            return await GetProfileTagFilterData(
                _context, profileId, ProfileSelectedTagFlags.Monitor);
        }

        public async Task<IEnumerable<NodeTag>> GetAllTags()
        {
            return await _context.Tags
                .AsNoTracking()
                .OrderBy(t => t.ID)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomWebService>> GetAllCws()
        {
            return await _context.WebServices
                .AsNoTracking()
                .OrderBy(cvs => cvs.ID)
                .ToListAsync();
        }

        public async Task<string> GetCwsBoundingString(int nodeId, int cwsId)
        {
            CustomWsBinding binding = await _context.WebServiceBindings.AsNoTracking()
                .Where(wsb => wsb.NodeID == nodeId && wsb.ServiceID == cwsId)
                .Include(wsb => wsb.Service)
                .Include(wsb => wsb.Node)
                .SingleOrDefaultAsync();
            if (binding == null)
            {
                return null;
            }

            string rawServiceString = binding.Service.ServiceStr;
            string ip = new System.Net.IPAddress(binding.Node.ip).ToString();
            KeyValuePair<string, string>[] paramSwap =
            {
                new KeyValuePair<string, string>("{param1}", binding.Param1),
                new KeyValuePair<string, string>("{param2}", binding.Param2),
                new KeyValuePair<string, string>("{param3}", binding.Param3),
                new KeyValuePair<string, string>("{node_ip}", ip),
            };
            StringBuilder buildServiceStr = new StringBuilder(rawServiceString);
            foreach (KeyValuePair<string, string> swapper in paramSwap)
            {
                buildServiceStr.Replace(swapper.Key, swapper.Value);
            }

            return buildServiceStr.ToString();
        }


        public async Task<Profile> CreateProfile(Profile profile)
        {
            Profile newProfile = new Profile
            {
                ID = 0,
                Name = profile.Name,
                StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
                DepthMonitoring = profile.DepthMonitoring,
                MonitorInterval = profile.MonitorInterval,
                RealTimePingUIUpdate = profile.RealTimePingUIUpdate
            };

            _context.Profiles.Add(newProfile);
            await _context.SaveChangesAsync();
            _context.Entry(newProfile).State = EntityState.Detached;
            return newProfile;
        }

        public async Task<NtwkNode> CreateNodeOnRoot(NtwkNode node)
        {
            NtwkNode newNode = new NtwkNode
            {
                ID = 0,
                Parent = null,
                ParentPort = node.ParentPort,
                Name = node.Name,
                ip = node.ip,
                OpenTelnet = node.OpenTelnet,
                OpenSSH = node.OpenSSH,
                OpenPing = node.OpenPing
            };
            _context.Nodes.Add(newNode);
            _context.NodesClosureTable.AddRange(
                await __CreateNewNodeClosures(_context, null, newNode.ID)
            );
            await _context.SaveChangesAsync();
            _context.Entry(newNode).State = EntityState.Detached;
            return newNode;
        }

        public async Task<NtwkNode> CreateNodeWithParent(NtwkNode node, int parentId)
        {
            NtwkNode newNode = new NtwkNode
            {
                ID = 0,
                ParentID = parentId,
                ParentPort = node.ParentPort,
                Name = node.Name,
                ip = node.ip,
                OpenTelnet = node.OpenTelnet,
                OpenSSH = node.OpenSSH,
                OpenPing = node.OpenPing
            };
            _context.Nodes.Add(newNode);
            _context.NodesClosureTable.AddRange(
                await __CreateNewNodeClosures(_context, parentId, newNode.ID)
            );
            await _context.SaveChangesAsync();
            _context.Entry(newNode).State = EntityState.Detached;
            newNode.Parent = null;
            return newNode;
        }

        public async Task<NodeTag> CreateTag(NodeTag tag)
        {
            NodeTag newTag = new NodeTag
            {
                ID = 0,
                Name = tag.Name
            };
            _context.Tags.Add(newTag);
            await _context.SaveChangesAsync();
            _context.Entry(newTag).State = EntityState.Detached;
            return newTag;
        }

        public async Task CreateWebServiceBinding(
            int nodeId,
            int cwsId,
            string param1,
            string param2,
            string param3
        )
        {
            CustomWsBinding binding = new CustomWsBinding
            {
                ID = 0,
                ServiceID = cwsId,
                NodeID = nodeId,
                Param1 = param1,
                Param2 = param2,
                Param3 = param3,
            };
            _context.WebServiceBindings.Add(binding);
            await _context.SaveChangesAsync();
        }

        public async Task SetNodeTags(int nodeId, IEnumerable<int> tagIDs)
        {
            IEnumerable<int> alreadyAttachedTagsIDs = await _context.TagAttachments
                .Where(a => a.NodeID == nodeId)
                .Select(a => a.TagID)
                .ToArrayAsync();
            var iDs = tagIDs as int[] ?? tagIDs.ToArray();
            IEnumerable<int> idsOfTagsToRemove = alreadyAttachedTagsIDs
                .Except(iDs);
            IEnumerable<TagAttachment> tagAttachmentsToRemove = await _context.TagAttachments
                .Where(a => a.NodeID == nodeId && idsOfTagsToRemove.Contains(a.TagID))
                .ToArrayAsync();
            IEnumerable<TagAttachment> newTagAttachments = iDs
                .Except(alreadyAttachedTagsIDs)
                .Select(id => new TagAttachment
                {
                    ID = 0,
                    TagID = id,
                    NodeID = nodeId,
                });

            _context.TagAttachments.RemoveRange(tagAttachmentsToRemove);
            _context.TagAttachments.AddRange(newTagAttachments);
            await _context.SaveChangesAsync();
        }

        private static async Task __SetProfileTagSelection(
            NtwkDBContext context,
            int profileId,
            IEnumerable<int> tagIDs,
            ProfileSelectedTagFlags flag
        )
        {
            IEnumerable<int> ids = tagIDs as int[] ?? tagIDs.ToArray();
            IEnumerable<ProfileSelectedTag> currentSelection =
                await context.ProfilesTagSelection
                    .Where(pts => pts.BindedProfileID == profileId)
                    .ToListAsync();
            var currentWithFlag = currentSelection
                .Where(pts => flag == (pts.Flags & flag))
                .Select(pts => new
                {
                    toStay = ids.Contains(pts.TagID),
                    pts
                })
                .GroupBy(t => t.toStay)
                .Select(collection => new
                {
                    collection.First().toStay,
                    col = collection.Select(t => t.pts)
                })
                .ToArray();
            IEnumerable<int> toStay = currentWithFlag
                .SingleOrDefault(t => t.toStay)
                ?.col?.Select(pst => pst.TagID).ToList();
            IEnumerable<ProfileSelectedTag> toReset = currentWithFlag
                .SingleOrDefault(t => !t.toStay)
                ?.col;
            toStay = toStay ?? (new List<int>());
            toReset = toReset ?? (new List<ProfileSelectedTag>());
            IEnumerable<ProfileSelectedTag> currentWithoutFlagToSet =
                currentSelection
                    .Where(pts => flag != (pts.Flags & flag))
                    .Where(pts => ids.Contains(pts.TagID))
                    .ToList();
            foreach (ProfileSelectedTag pst in currentWithoutFlagToSet)
            {
                pst.Flags = pst.Flags | flag;
            }

            List<ProfileSelectedTag> toRemove = new List<ProfileSelectedTag>();
            foreach (ProfileSelectedTag pst in toReset)
            {
                if (ProfileSelectedTagFlags.None == (pst.Flags ^ flag))
                    toRemove.Add(pst);
                else
                    pst.Flags = pst.Flags ^ flag;
            }

            context.RemoveRange(toRemove);

            if ((toStay.Count() + currentWithoutFlagToSet.Count()) < ids.Count())
            {
                IEnumerable<int> alreadySetTags = new[]
                    {
                        toStay,
                        currentWithoutFlagToSet.Select(pst => pst.TagID),
                    }
                    .SelectMany(t => t);
                IEnumerable<int> notSetTags = ids.Except(alreadySetTags);
                IEnumerable<ProfileSelectedTag> newSelections = notSetTags
                    .Select(tagId => new ProfileSelectedTag
                    {
                        ID = 0,
                        Flags = flag,
                        BindedProfileID = profileId,
                        TagID = tagId
                    });
                context.ProfilesTagSelection.AddRange(newSelections);
            }

            await context.SaveChangesAsync();
        }

        private static async Task __SetOneSelectionToAnother(
            NtwkDBContext context,
            int profileId,
            ProfileSelectedTagFlags from,
            ProfileSelectedTagFlags to
        )
        {
            IEnumerable<ProfileSelectedTag> fromAndToSelections =
                await context.ProfilesTagSelection
                    .Where(pst => pst.BindedProfileID == profileId
                                  && (
                                      from == (pst.Flags & from) ||
                                      to == (pst.Flags & to))
                    )
                    .ToListAsync();
            IEnumerable<ProfileSelectedTag> toSet = fromAndToSelections
                .Where(pst => from == (pst.Flags & from) &&
                              to != (pst.Flags & to));
            IEnumerable<ProfileSelectedTag> toRemove = fromAndToSelections
                .Where(pst => ProfileSelectedTagFlags.None == (pst.Flags ^ to));
            foreach (ProfileSelectedTag pst in toSet)
            {
                pst.Flags = pst.Flags | to;
            }

            context.ProfilesTagSelection.RemoveRange(toRemove);
            await context.SaveChangesAsync();
        }

        public async Task SetProfileViewTagsSelection(int profileId, IEnumerable<int> tagIDs)
        {
            await __SetProfileTagSelection(_context, profileId, tagIDs,
                ProfileSelectedTagFlags.NodesListView);
        }


        public async Task SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileId)
        {
            await __SetOneSelectionToAnother(_context,
                profileId,
                ProfileSelectedTagFlags.Monitor,
                ProfileSelectedTagFlags.NodesListView
            );
        }

        public async Task SetProfileMonitorTagsSelection(int profileId, IEnumerable<int> tagIDs)
        {
            await __SetProfileTagSelection(_context, profileId, tagIDs,
                ProfileSelectedTagFlags.Monitor);
        }

        public async Task SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileId)
        {
            await __SetOneSelectionToAnother(_context,
                profileId,
                ProfileSelectedTagFlags.NodesListView,
                ProfileSelectedTagFlags.Monitor
            );
        }

        public async Task<CustomWebService> CreateCustomWebService(CustomWebService cws)
        {
            CustomWebService newCws = new CustomWebService
            {
                ID = 0,
                Name = cws.Name,
                ServiceStr = cws.ServiceStr,
                Parametr1Name = cws.Parametr1Name,
                Parametr2Name = cws.Parametr2Name,
                Parametr3Name = cws.Parametr3Name,
            };
            _context.WebServices.Add(newCws);
            await _context.SaveChangesAsync();
            _context.Entry(newCws).State = EntityState.Detached;
            return newCws;
        }

        public async Task UpdateProfile(Profile newProfileData)
        {
            Profile profile = await _context.Profiles.FindAsync(newProfileData.ID);
            profile.Name = newProfileData.Name;
            profile.StartMonitoringOnLaunch = newProfileData.StartMonitoringOnLaunch;
            profile.DepthMonitoring = newProfileData.DepthMonitoring;
            profile.MonitorInterval = newProfileData.MonitorInterval;
            profile.RealTimePingUIUpdate = newProfileData.RealTimePingUIUpdate;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTag(NodeTag newNodeTagData)
        {
            NodeTag tag = await _context.Tags.FindAsync(newNodeTagData.ID);
            tag.Name = newNodeTagData.Name;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNode(NtwkNode newNodeData)
        {
            NtwkNode node = await _context.Nodes.FindAsync(newNodeData.ID);
            node.Name = newNodeData.Name;
            node.ParentPort = newNodeData.ParentPort;
            node.ip = newNodeData.ip;
            node.OpenPing = newNodeData.OpenPing;
            node.OpenSSH = newNodeData.OpenSSH;
            node.OpenTelnet = newNodeData.OpenTelnet;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomWebService(CustomWebService cws)
        {
            CustomWebService cwsToUpdate =
                await _context.WebServices.FindAsync(cws.ID);
            cwsToUpdate.Name = cws.Name;
            cwsToUpdate.ServiceStr = cws.ServiceStr;
            cwsToUpdate.Parametr1Name = cws.Parametr1Name;
            cwsToUpdate.Parametr2Name = cws.Parametr2Name;
            cwsToUpdate.Parametr3Name = cws.Parametr3Name;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWebServiceBinding(
            int nodeId,
            int cwsId,
            string param1,
            string param2,
            string param3
        )
        {
            var binding = await _context.WebServiceBindings
                .SingleAsync(wsb => wsb.NodeID == nodeId && wsb.ServiceID == cwsId);
            binding.Param1 = param1;
            binding.Param2 = param2;
            binding.Param3 = param3;
            await _context.SaveChangesAsync();
        }


        public async Task<Profile> RemoveProfile(int profileId)
        {
            Profile profile = await _context.Profiles
                .Include(p => p.MonitoringSessions)
                .ThenInclude(s => s.Pulses)
                .ThenInclude(p => p.Messages)
                .Include(p => p.FilterTagSelection)
                .SingleAsync(p => p.ID == profileId);
            IEnumerable<ProfileSelectedTag> viewTagsSelection = profile.FilterTagSelection;
            IEnumerable<MonitoringSession> sessions = profile.MonitoringSessions;
            IEnumerable<MonitoringPulseResult> pulses = sessions.SelectMany(s => s.Pulses).ToArray();
            IEnumerable<MonitoringMessage> messages = pulses.SelectMany(p => p.Messages);
            _context.ProfilesTagSelection.RemoveRange(viewTagsSelection);
            _context.MonitoringMessages.RemoveRange(messages);
            _context.MonitoringPulses.RemoveRange(pulses);
            _context.MonitoringSessions.RemoveRange(sessions);
            profile.MonitoringSessions = null;
            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<NtwkNode> RemoveNode(int nodeId)
        {
            NtwkNode node = await _context.Nodes
                .Include(n => n.Children)
                .SingleAsync(n => n.ID == nodeId);
            if (node.Children.Count != 0)
            {
                throw new InvalidOperationException();
            }

            _context.Nodes.Remove(node);
            await _context.SaveChangesAsync();
            return node;
        }

        public async Task<NodeTag> RemoveTag(int tagId)
        {
            NodeTag tag = await _context.Tags
                .Include(t => t.Attachments)
                .Include(t => t.ProfilesFilterSelections)
                .SingleAsync(t => t.ID == tagId);
            _context.RemoveRange(tag.Attachments);
            _context.RemoveRange(tag.ProfilesFilterSelections);
            tag.Attachments = null;
            tag.ProfilesFilterSelections = null;
            _context.Remove(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<CustomWebService> RemoveCustomWebService(int cwsId)
        {
            CustomWebService service = await _context.WebServices
                .Include(ws => ws.Bindings)
                .SingleAsync(ws => ws.ID == cwsId);
            _context.RemoveRange(service.Bindings);
            service.Bindings = null;
            _context.Remove(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task RemoveWebServiceBinding(
            int nodeId,
            int cwsId
        )
        {
            var binding = await _context.WebServiceBindings
                .SingleAsync(wsb => wsb.NodeID == nodeId && wsb.ServiceID == cwsId);
            _context.WebServiceBindings.Remove(binding);
            await _context.SaveChangesAsync();
        }
    }
}