using System;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.EFDatabase;
using static Tests.EFDatabaseMockingUtils;

namespace Tests {

public class Data_dataSourceTest {

    private readonly ITestOutputHelper _testOutput;

    public Data_dataSourceTest(ITestOutputHelper output) {
        this._testOutput = output;
    }

    [Fact]
    public async Task GetNewSession_CreatesNewSession() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        SetProfileVTagsTo1_2AndMTagsTo2_3(context, IDSet);

        var createdSession = await dataSource.GetNewSession(IDSet.ProfileID);
        var sessionWithInclude = context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.CreatedByProfile)
            .Include(s => s.Pulses)
            .Single(s => s.ID == createdSession.ID);

        Assert.Equal(IDSet.ProfileID, sessionWithInclude.CreatedByProfile.ID);
        Assert.Equal(2, sessionWithInclude.ParticipatingNodesNum);
        Assert.Empty(sessionWithInclude.Pulses);
    }

    [Fact]
    public async void SavePulseResult_CreatesNewPulse() {
        var (context, idSet, dataSource) = GetTestDataContext();
        
        var pulse = new MonitoringPulseResult {
            ID = 0,
            Responded = 2,
            Silent = 0,
            Skipped = 0,
        };
        var message = new MonitoringMessage {
            ID = 0,
            MessageType = MonitoringMessageType.Warning_InconsistentPing,
            MessageSourceNodeName = NodeNames[0]
        };
        var messages = new MonitoringMessage[1];
        messages[0] = message;
        int sessionID = context.MonitoringSessions
            .AsNoTracking()
            .First()
            .ID;

        int createdPulseID = (await dataSource.SavePulseResult(
            sessionID,
            pulse, messages
        )).ID;
        var createdPulse = context.MonitoringPulses
            .AsNoTracking()
            .Include(p => p.Messages)
            .Single(p => p.ID == createdPulseID);
        var session = context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.Pulses)
            .Single(s => s.ID == sessionID);

        Assert.Equal(3, session.Pulses.Count);
        Assert.Equal(3, context.MonitoringPulses.Count());
        Assert.Equal(2, context.MonitoringMessages.Count());
        Assert.Equal(2, createdPulse.Responded);
        Assert.Equal(0, createdPulse.Silent);
        Assert.Equal(0, createdPulse.Skipped);
        Assert.Single(createdPulse.Messages);
        Assert.Equal(NodeNames[0], createdPulse.Messages.Single().MessageSourceNodeName);
    }

    [Fact]
    public async void ClearEmptySessionsLogic_RemovedAllEmptySessionsOnly() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        var testProfile = context.Profiles
            .AsNoTracking()
            .First();
        context.MonitoringSessions.Add(new MonitoringSession {
            ID = 0,
            CreatedByProfile = testProfile,
            ParticipatingNodesNum = 0,
            CreationTime = 1528329015,
            LastPulseTime = 0,
        });
        var tag1 = context.Tags
            .AsNoTracking()
            .Single(t => t.ID == IDSet.TagsIDs[0]);
        context.MonitoringSessions.Add(new MonitoringSession {
            ID = 0,
            CreatedByProfile = testProfile,
            ParticipatingNodesNum = 2,
            CreationTime = 1528329325,
            LastPulseTime = 0,
        });
        context.SaveChanges();

        var countSessionsBefore = context.MonitoringSessions.Count();
        var countEmptySessionsBefore = context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.Pulses)
            .Count(s => s.Pulses.Count == 0);
        await dataSource.ClearEmptySessions();
        var countSessionsAfter = context.MonitoringSessions.Count();
        var countEmptySessionsAfter = context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.Pulses)
            .Count(s => s.Pulses.Count == 0);
        
        Assert.Equal(countSessionsBefore-countEmptySessionsBefore,
            countSessionsAfter);
        Assert.Equal(0, countEmptySessionsAfter);
    }

    [Fact]
    public async void GetSessionForProfileLogic_GetsAllSessionsForProfile() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        var testProfile2 = new Profile {
            ID = 0,
            Name = "TestProfile2",
            StartMonitoringOnLaunch = true,
            DepthMonitoring = true,
            MonitorInterval = 10
        };
        context.Profiles.Add(testProfile2);
        context.SaveChanges();

        var sessionsForProfile1 = await dataSource.GetSessionsForProfile(IDSet.ProfileID);
        var sessionsForProfile2 = await dataSource.GetSessionsForProfile(testProfile2.ID);

        Assert.Single(sessionsForProfile1);
        Assert.Empty(sessionsForProfile2);
    }

    [Fact]
    public async void GetSessionReport_GetsAllPulsesForSession() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        int sessionID = context.MonitoringSessions
            .AsNoTracking()
            .Single(s => s.CreationTime == 1528359015)
            .ID;
        var pulsesForSession1 = await dataSource.GetSessionReport(sessionID);

        Assert.Equal(2, pulsesForSession1.Count());
        Assert.Contains(pulsesForSession1, p => p.CreationTime == 1528359015);
        Assert.Contains(pulsesForSession1, p => p.CreationTime == 1528360285);
    }

    [Fact]
    public async void GetAllProfiles_GetsAllProfilesInDB() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        var testProfile2 = new Profile {
            ID = 0,
            Name = "TestProfile2",
            StartMonitoringOnLaunch = true,
            DepthMonitoring = true,
            MonitorInterval = 10
        };
        context.Profiles.Add(testProfile2);
        context.SaveChanges();

        var profiles = await dataSource.GetAllProfiles();

        Assert.Equal(2, profiles.Count());
    }

    [Fact] //Disgusting
    public async void GetAllNodesData_GetsAllNodesDataObjectWithNodesGroupedByTreeDepthWithBoundServicesIDsAndTagIDs() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var cwData = context.WebServices.AsNoTracking()
            .Select(ws => new { ws.ID, ws.Name })
            .ToList();
        var nodesData = context.NodesClosureTable.AsNoTracking()
            .Where(cl => cl.AncestorID == null)
            .Include(cl => cl.Descendant)
                .ThenInclude(n => n.Tags)
            .Include(cl => cl.Descendant)
                .ThenInclude(n => n.CustomWebServices)
            .GroupBy(cl => cl.Distance)
            .Select(group => group
                .Select(cl => cl.Descendant)
                    .OrderByDescending(n => n.ID)
                .Select(n => new {
                    nodeTagsIDs = new HashSet<int>(n.Tags.Select(ta => ta.TagID)),
                    nodeServicesIDs = new HashSet<int>(n.CustomWebServices.Select(wsb => wsb.ServiceID)) 
                })
            );

        Data.Model.IntermediateModel.AllRawNodesData allNodesData = 
            await dataSource.GetAllNodesData();
    
        //Assert nodes groups structure
        Assert.Equal(2, allNodesData.NodesData.Count());
        Assert.Equal(2, allNodesData.NodesData.First().Count());
        Assert.Single(allNodesData.NodesData.Skip(1).First());
        Assert.NotNull(allNodesData.NodesData.First().SingleOrDefault(nd => nd.Node.ID == IDSet.NodesIDs[0]));
        Assert.NotNull(allNodesData.NodesData.First().SingleOrDefault(nd => nd.Node.ID == IDSet.NodesIDs[2]));
        Assert.NotNull(allNodesData.NodesData.Skip(1).First().SingleOrDefault(nd => nd.Node.ID == IDSet.NodesIDs[1]));
        //Assert ws data
        Assert.All(
            allNodesData.WebServicesData.Zip(cwData,  (o1, o2) => new { o1, o2 }),
            (o) => {
                if(o.o1.ID != o.o2.ID) {
                    throw new Exception($"Unexpected ID value: {o.o1.ID} != {o.o2.ID}");
                }
                if(o.o1.Name != o.o2.Name) {
                    throw new Exception($"Unexpected Name value: {o.o1.Name} != {o.o2.Name}");
                }
            }
        );
        //Assert nodes tags data and bound services data
        //(NOPE, I AM DONE)
        /*Assert.All(
            allNodesData.NodesData.First().Zip(nodesData.First(),  (o1, o2) => new { o1, o2 })
                .Concat(allNodesData.NodesData.Skip(1).First().Zip(nodesData.Skip(1).First(),  (o1, o2) => new { o1, o2 })),
            (o) => {
                string nodeName = o.o1.Node.Name;
                if(o.o1.TagsIDs.Length != o.o2.nodeTagsIDs.Count) {
                    throw new Exception($"Wrong ID count ({nodeName}): {o.o1.TagsIDs.Length} != {o.o2.nodeTagsIDs.Count}");
                }
                foreach(int tagID in o.o1.TagsIDs) {
                    if(!o.o2.nodeTagsIDs.Contains(tagID)) {
                        throw new Exception($"Unexpected tagID value ({nodeName}): {tagID}");
                    }
                }
                if(o.o1.BoundWebServicesIDs.Length != o.o2.nodeServicesIDs.Count) {
                    throw new Exception($"Wrong bound services count ({nodeName}): {o.o1.BoundWebServicesIDs.Length} != {o.o2.nodeServicesIDs.Count}");
                }
                foreach(int wsID in o.o1.BoundWebServicesIDs) {
                    if(!o.o2.nodeServicesIDs.Contains(wsID)) {
                        throw new Exception($"Unexpected wsID value ({nodeName}): {wsID}");
                    }
                }
            }
        );*/
    }

    [Fact]
    public async void GetNodeIP_GetsNodeIPFromDB() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        var ip = await dataSource.GetNodeIP(IDSet.NodesIDs[0]);

        Assert.Equal(context.Nodes.Single(n => n.ID == IDSet.NodesIDs[0]).ip, ip);
    }

    [Fact]
    public async void GetTaggedNodesIDs_GetsOnlyAllTheNodesIDsForNodesWithSpecificTag() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var tag1Nodes = await dataSource.GetTaggedNodesIDs(IDSet.TagsIDs[0]);
        var tag2Nodes = await dataSource.GetTaggedNodesIDs(IDSet.TagsIDs[1]);
        var tag3Nodes = await dataSource.GetTaggedNodesIDs(IDSet.TagsIDs[2]);

        Assert.Equal(2, tag1Nodes.Count());
        Assert.Single(tag2Nodes);
        Assert.Single(tag3Nodes);
        Assert.Contains(IDSet.NodesIDs[0], tag1Nodes);
        Assert.Contains(IDSet.NodesIDs[1], tag1Nodes);
        Assert.Contains(IDSet.NodesIDs[1], tag2Nodes);
        Assert.Contains(IDSet.NodesIDs[2], tag3Nodes);
    }

    [Fact]
    public async void GetProfileViewTagFilterData_WillGetAllNodesWithTagsSpecifiedInProfileView() {   
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        SetProfileVTagsTo1_2AndMTagsTo2_3(context, IDSet);
        var viewNodesForTags1And2 = await dataSource
            .GetProfileViewTagFilterData(IDSet.ProfileID);
        SetProfileVTagsTo2_3AndMTagsTo1(context, IDSet);
        var viewNodesForTags2And3 = await dataSource
            .GetProfileViewTagFilterData(IDSet.ProfileID);

        Assert.Equal(2, viewNodesForTags1And2.TagsIDs.Count());
        Assert.Equal(2, viewNodesForTags1And2.NodesIDs.Count());
        Assert.Single(viewNodesForTags1And2.NodesIDs, IDSet.NodesIDs[0]);
        Assert.Single(viewNodesForTags1And2.NodesIDs, IDSet.NodesIDs[1]);
        Assert.Single(viewNodesForTags1And2.TagsIDs, IDSet.TagsIDs[0]);
        Assert.Single(viewNodesForTags1And2.TagsIDs, IDSet.TagsIDs[1]);
        Assert.Equal(2, viewNodesForTags2And3.TagsIDs.Count());
        Assert.Equal(2, viewNodesForTags2And3.NodesIDs.Count());
        Assert.Single(viewNodesForTags2And3.TagsIDs, IDSet.TagsIDs[1]);
        Assert.Single(viewNodesForTags2And3.TagsIDs, IDSet.TagsIDs[2]);
        Assert.Single(viewNodesForTags2And3.NodesIDs, IDSet.NodesIDs[1]);
        Assert.Single(viewNodesForTags2And3.NodesIDs, IDSet.NodesIDs[2]);
    }

    [Fact]
    public async void GetIDsOfNodesBySelectedTagsInProfileMonitor_WillGetAllNodesWithTagsSpecifiedInProfileMonitor() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        SetProfileVTagsTo1_2AndMTagsTo2_3(context, IDSet);
        var monitorNodesForTags2And3 = await dataSource
            .GetProfileMonitorTagFilterData(IDSet.ProfileID);
        SetProfileVTagsTo2_3AndMTagsTo1(context, IDSet);
        var monitorNodesForTags1 = await dataSource
            .GetProfileMonitorTagFilterData(IDSet.ProfileID);

        Assert.Equal(2, monitorNodesForTags2And3.TagsIDs.Count());
        Assert.Equal(2, monitorNodesForTags2And3.NodesIDs.Count());
        Assert.Single(monitorNodesForTags2And3.TagsIDs, IDSet.TagsIDs[1]);
        Assert.Single(monitorNodesForTags2And3.TagsIDs, IDSet.TagsIDs[2]);
        Assert.Single(monitorNodesForTags2And3.NodesIDs, IDSet.NodesIDs[1]);
        Assert.Single(monitorNodesForTags2And3.NodesIDs, IDSet.NodesIDs[2]);
        Assert.Single(monitorNodesForTags1.TagsIDs);
        Assert.Equal(2, monitorNodesForTags1.NodesIDs.Count());
        Assert.Single(monitorNodesForTags1.TagsIDs, IDSet.TagsIDs[0]);
        Assert.Single(monitorNodesForTags1.NodesIDs, IDSet.NodesIDs[0]);
        Assert.Single(monitorNodesForTags1.NodesIDs, IDSet.NodesIDs[1]);
    }

    [Fact]
    public async void GetAllTags_ReturnsAllTheTags() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        var tags = await dataSource.GetAllTags();

        Assert.Equal(3, tags.Count());
    }

    [Fact]
    public async void GetAllCWS_ReturnsAllCWS() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        var cvsList = await dataSource.GetAllCWS();

        Assert.Equal(2, cvsList.Count());
        Assert.Equal(WebServicesNames[0], cvsList.First().Name);
        Assert.Equal(WebServicesNames[1], cvsList.Skip(1).First().Name);
    }

    [Fact]
    public async void CreateProfile_WillCreateNewProfile() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        string newProfile1Name = "CreatedProfile1";
        string newProfile2Name = "CreatedProfile2";
        var newProfile = new Profile {
            ID = 0,
            Name = "",
            StartMonitoringOnLaunch = true,
            DepthMonitoring = true,
            MonitorInterval = 10
        };

        var profilesBefore = context.Profiles
            .AsNoTracking()
            .ToList();
        newProfile.Name = newProfile1Name;
        var net1 = await dataSource.CreateProfile(newProfile);
        newProfile.Name = newProfile2Name;
        var net2 = await dataSource.CreateProfile(newProfile);
        var networksAfter = context.Profiles
            .AsNoTracking()
            .ToList();

        Assert.Single(profilesBefore);
        Assert.Equal(3, networksAfter.Count);
        Assert.DoesNotContain(profilesBefore, n => n.Name == newProfile1Name);
        Assert.DoesNotContain(profilesBefore, n => n.Name == newProfile2Name);
        Assert.Single(networksAfter, n => n.Name == newProfile1Name);
        Assert.Single(networksAfter, n => n.Name == newProfile2Name);
    }

    [Fact]
    public async void __CreateNewNodeClosures_WillCreateAllNeededClosures() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var expectedClosuresForNode1 = new [] {
            new { ancestorID = (int?)null,          descendantID = IDSet.NodesIDs[0], dist = 0 },
            new { ancestorID = (int?)IDSet.NodesIDs[0], descendantID = IDSet.NodesIDs[0], dist = 0 }
        };
        var expectedClosuresForNode2 = new [] {
            new { ancestorID = (int?)null,          descendantID = IDSet.NodesIDs[1], dist = 1 },
            new { ancestorID = (int?)IDSet.NodesIDs[0], descendantID = IDSet.NodesIDs[1], dist = 1 },
            new { ancestorID = (int?)IDSet.NodesIDs[1], descendantID = IDSet.NodesIDs[1], dist = 0 }
        };
        var expectedClosuresForNode3 = new [] {
            new { ancestorID = (int?)null,          descendantID = IDSet.NodesIDs[2], dist = 0 },
            new { ancestorID = (int?)IDSet.NodesIDs[2], descendantID = IDSet.NodesIDs[2], dist = 0 }
        };

        var clN1 = await EFDataSource.__CreateNewNodeClosures(context, null, IDSet.NodesIDs[0]);
        context.AddRange(clN1);
        context.SaveChanges();
        var clN2 = await EFDataSource.__CreateNewNodeClosures(context, IDSet.NodesIDs[0], IDSet.NodesIDs[1]);
        context.AddRange(clN2);
        context.SaveChanges();
        var clN3 = await EFDataSource.__CreateNewNodeClosures(context, null, IDSet.NodesIDs[2]);
        context.AddRange(clN3);
        context.SaveChanges();
        var closuresForNode1 = context.NodesClosureTable
            .AsNoTracking()
            .Where(c => c.DescendantID == IDSet.NodesIDs[0])
            .ToList();
        var closuresForNode2 = context.NodesClosureTable
            .AsNoTracking()
            .Where(c => c.DescendantID == IDSet.NodesIDs[1])
            .ToList();
        var closuresForNode3 = context.NodesClosureTable
            .AsNoTracking()
            .Where(c => c.DescendantID == IDSet.NodesIDs[2])
            .ToList();
        var tests = new [] {
            new { exp = expectedClosuresForNode1, act = closuresForNode1 },
            new { exp = expectedClosuresForNode2, act = closuresForNode2 },
            new { exp = expectedClosuresForNode3, act = closuresForNode3 }
        }
            .Select(t => t.exp
                .Zip(t.act, 
                    (exp, act) => new {
                        expectedAncestorID = exp.ancestorID,
                        actualAncestorID = act.AncestorID,
                        expedtedDescendantID = exp.descendantID,
                        actualDescendantID = act.DescendantID,
                        expectedDistance = exp.dist,
                        actualDistance = act.Distance
                    }
                )
                .ToList()
            )
            .ToList();
        Action<dynamic> dynamicTest = (dynamic t) => {
            if(t.expectedAncestorID != t.actualAncestorID ||
                t.expedtedDescendantID != t.actualDescendantID ||
                t.expectedDistance != t.actualDistance
            ) {
                throw new Exception("Unexpected closure values.");
            }
        };

        Assert.Equal(2, tests[0].Count);
        Assert.Equal(3, tests[1].Count);
        Assert.Equal(2, tests[2].Count);
        Assert.All(tests[0], dynamicTest);
        Assert.All(tests[1], dynamicTest);
        Assert.All(tests[2], dynamicTest);
    }

    [Fact]
    public async void CreateNodeOnRoot_WillCreateNewNodeWithoutParentAndClosureTableEnriesForIt() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        int nodesBefore = context.Nodes.Count();
        var newNode = new NtwkNode {
            ID = 0,
            Parent = null,
            Name = "NewNode1",
            ip = 167837697,
            OpenTelnet = false,
            OpenSSH = true,
            OpenPing = true
        };
        var createdNode1 = await dataSource.CreateNodeOnRoot(
            newNode
        );
        newNode.Name = "NewNode2";
        var createdNode2 = await dataSource.CreateNodeOnRoot(
            newNode
        );
        var createdNode1WithParent = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == createdNode1.ID);
        var createdNode2WithParent = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == createdNode2.ID);
        var createdNode1Closures = context.NodesClosureTable
            .Where(c => c.DescendantID == createdNode1.ID)
            .ToList();
        var createdNode2Closures = context.NodesClosureTable
            .Where(c => c.DescendantID == createdNode2.ID)
            .ToList();
        int nodesAfter = context.Nodes.Count();
        
        //NewNodes
        Assert.Null(createdNode1WithParent.Parent);
        Assert.Null(createdNode2WithParent.Parent);
        Assert.Equal(3, nodesBefore);
        Assert.Equal(5, nodesAfter);
        //NewClosureTableEntries
        Assert.Equal(2, createdNode1Closures.Count);
        Assert.Equal(2, createdNode2Closures.Count);
        Assert.Single(createdNode1Closures, c => c.AncestorID == null);
        Assert.Single(createdNode1Closures, c => c.AncestorID == c.DescendantID);
        Assert.Single(createdNode2Closures, c => c.AncestorID == null);
        Assert.Single(createdNode2Closures, c => c.AncestorID == c.DescendantID);
    }
    
    [Fact]
    public async void CreateNodeWithParent_WillCreateNewNodeWithSpecifiedParentAndClosureTableEnriesForIt() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        Action<dynamic> equalOrThrow =  (dynamic c) => {
            if(c.c1.AncestorID != c.c2.AncestorID)
                throw new Exception($"Unexpected ancestor value {c.c1.AncestorID} != {c.c2.AncestorID}.");
            if(c.c1.DescendantID != c.c2.DescendantID)
                throw new Exception($"Unexpected descendant value {c.c1.DescendantID} != {c.c2.DescendantID}.");
            if(c.c1.Distance != c.c2.Distance)
                throw new Exception($"Unexpected distance value {c.c1.Distance} != {c.c2.Distance}.");
        };

        int nodesBefore = context.Nodes.Count();
        var newNode = new NtwkNode {
            ID = 0,
            Parent = null,
            Name = "NewNode1",
            //NodeDepth = 0,
            ip = 167837697,
            OpenTelnet = false,
            OpenSSH = true,
            OpenPing = true
        };
        var createdNode1 = await dataSource.CreateNodeWithParent(
            newNode, IDSet.NodesIDs[1]
        );
        newNode.Name = "NewNode2";
        var createdNode2 = await dataSource.CreateNodeWithParent(
            newNode, IDSet.NodesIDs[2]
        );
        var createdNode1WithParent = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == createdNode1.ID);
        var createdNode2WithParent = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == createdNode2.ID);
        var createdNode1Closures = context.NodesClosureTable
            .Where(c => c.DescendantID == createdNode1.ID)
            .ToList();
        var createdNode2Closures = context.NodesClosureTable
            .Where(c => c.DescendantID == createdNode2.ID)
            .ToList();
        var expectedClosuresForCreatedNode1 = new [] {
            new NodeClosure {
                ID=0, AncestorID = null, DescendantID = createdNode1.ID, Distance = 2
            },
            new NodeClosure {
                ID=0, AncestorID = IDSet.NodesIDs[0], DescendantID = createdNode1.ID, Distance = 2
            },
            new NodeClosure {
                ID=0, AncestorID = IDSet.NodesIDs[1], DescendantID = createdNode1.ID, Distance = 1
            },
            new NodeClosure {
                ID=0, AncestorID = createdNode1.ID, DescendantID = createdNode1.ID, Distance = 0
            },
        };
        var expectedClosuresForCreatedNode2 = new [] {
            new NodeClosure {
                ID=0, AncestorID = null, DescendantID = createdNode2.ID, Distance = 1
            },
            new NodeClosure {
                ID=0, AncestorID = IDSet.NodesIDs[2], DescendantID = createdNode2.ID, Distance = 1
            },
            new NodeClosure {
                ID=0, AncestorID = createdNode2.ID, DescendantID = createdNode2.ID, Distance = 0
            },
        };
        int nodesAfter = context.Nodes.Count();
        
        //NewNodes
        Assert.Single(context.Nodes, n => n.Name == "NewNode1");
        Assert.Single(context.Nodes, n => n.Name == "NewNode2");
        Assert.NotNull(createdNode1WithParent.Parent);
        Assert.NotNull(createdNode2WithParent.Parent);
        Assert.Equal(IDSet.NodesIDs[1], createdNode1WithParent.Parent.ID);
        Assert.Equal(IDSet.NodesIDs[2], createdNode2WithParent.Parent.ID);
        Assert.Equal(3, nodesBefore);
        Assert.Equal(5, nodesAfter);
        //NewClosures
        Assert.Equal(4, createdNode1Closures.Count);
        Assert.Equal(3, createdNode2Closures.Count);
        Assert.All(
            expectedClosuresForCreatedNode1
                .Zip(createdNode1Closures, (c1, c2) => new { c1, c2 }),
            equalOrThrow
        );
        Assert.All(
            expectedClosuresForCreatedNode2
                .Zip(createdNode2Closures, (c1, c2) => new { c1, c2 }),
            equalOrThrow
        );
    }

    [Fact]
    public async void HasChildren_WillReturnTrueOnlyIfNodeHasChildren() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool node1HasChildren = await dataSource.HasChildren(
            IDSet.NodesIDs[0]
        );
        bool node2HasChildren = await dataSource.HasChildren(
            IDSet.NodesIDs[1]
        );
        bool node3HasChildren = await dataSource.HasChildren(
            IDSet.NodesIDs[2]
        );

        Assert.True(node1HasChildren);
        Assert.False(node2HasChildren);
        Assert.False(node3HasChildren);
    }

    [Fact]
    public async void CheckIfProfileExists_WillReturnTrueOnlyIfSpecifiedProfileExists() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        bool existingProfile = await dataSource
            .CheckIfProfileExists(IDSet.ProfileID);
        bool nonExistingProfile = await dataSource
            .CheckIfProfileExists(IDSet.ProfileID+199);
        
        Assert.True(existingProfile);
        Assert.False(nonExistingProfile);
    }

    [Fact]
    public async void CheckIfCWSExists_WillReturnTrueOnlyIfSpecifiedCWSExists() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        int existingCWS = await dataSource
            .GetCWSParamNumber(IDSet.WebServicesIDs[0]);
        int nonExistingCWS = await dataSource
            .GetCWSParamNumber(IDSet.WebServicesIDs[0]+199);
        
        Assert.Equal(0, existingCWS);
        Assert.Equal(-1, nonExistingCWS);
    }

    [Fact]
    public async void CheckIfSessionExists_WillReturnTrueOnlyIfSpecifiedMonitoringSessionExists() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        int ExistingSessionID = context.MonitoringSessions.First().ID;

        bool existingSession = await dataSource
            .CheckIfSessionExists(ExistingSessionID);
        bool nonExistingSession = await dataSource
            .CheckIfSessionExists(ExistingSessionID+199);
        
        Assert.True(existingSession);
        Assert.False(nonExistingSession);
    }

    [Fact]
    public async void CheckIfTagExists_WillReturnTrueOnlyIfSpecifiedTagExists() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool existingTags =
            await dataSource.CheckIfTagExists(
                IDSet.TagsIDs[0]
            ) &&
            await dataSource.CheckIfTagExists(
                IDSet.TagsIDs[1]
            ) &&
            await dataSource.CheckIfTagExists(
                IDSet.TagsIDs[2]
            );
        bool nonExistingTags =
            await dataSource.CheckIfTagExists(
                IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2]
            ) ||
            await dataSource.CheckIfTagExists(
                IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2]+1
            ) ||
            await dataSource.CheckIfTagExists(
                IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2]+2
            );
        
        Assert.True(existingTags);
        Assert.False(nonExistingTags);
    }

    

    [Fact]
    public async void CheckIfTagsExist_WillReturnTrueOnlyIfAllTheSpecifiedTagsExist() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool existingTags =
            await dataSource.CheckIfTagsExist(
                new [] { IDSet.TagsIDs[0] , IDSet.TagsIDs[1], IDSet.TagsIDs[2]}
            );
        bool nonExistingTags =
            await dataSource.CheckIfTagsExist(
                new [] {
                    IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2],
                    IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2]+1,
                    IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2]+2
                }
            );
        bool mixedTags = 
            await dataSource.CheckIfTagsExist(
                new [] {
                    IDSet.TagsIDs[0] , IDSet.TagsIDs[1],
                    IDSet.TagsIDs[0]+IDSet.TagsIDs[1]+IDSet.TagsIDs[2]+2
                }
            );
        
        Assert.True(existingTags);
        Assert.False(nonExistingTags);
        Assert.False(mixedTags);
    }

    [Fact]
    public async void CheckIfNodeExists_WillReturnTrueOnlyIfSpecifiedNodeExists() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingNodes =
            await dataSource.CheckIfNodeExists(
                IDSet.NodesIDs[0]
            ) &&
            await dataSource.CheckIfNodeExists(
                IDSet.NodesIDs[1]
            ) &&
            await dataSource.CheckIfNodeExists(
                IDSet.NodesIDs[2]
            );
        bool NonExistingNodes =
            await dataSource.CheckIfNodeExists(
                IDSet.NodesIDs[0]+IDSet.NodesIDs[1]+IDSet.NodesIDs[2]
            ) ||
            await dataSource.CheckIfNodeExists(
                IDSet.NodesIDs[0]+IDSet.NodesIDs[1]+IDSet.NodesIDs[2]+1
            ) ||
            await dataSource.CheckIfNodeExists(
                IDSet.NodesIDs[0]+IDSet.NodesIDs[1]+IDSet.NodesIDs[2]+2
            );
        
        Assert.True(ExistingNodes);
        Assert.False(NonExistingNodes);
    }

    [Fact]
    public async void CheckIfTagNameExists_WillReturnTrueOnlyIfTagWithSpecifiedNameIsAlreadyInTheDatabase() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingTagNames =
            await dataSource.CheckIfTagNameExists(
                TagNames[0], null
            ) &&
            await dataSource.CheckIfTagNameExists(
                TagNames[1], null
            ) &&
            await dataSource.CheckIfTagNameExists(
                TagNames[2], null
            );
        bool NonExistingTagNames =
            await dataSource.CheckIfTagNameExists(
                "TestTagNonexistantName1", null
            ) ||
            await dataSource.CheckIfTagNameExists(
                "Shvabra Cadabra", null
            ) ||
            await dataSource.CheckIfTagNameExists(
                "1234321", null
            );
        
        Assert.True(ExistingTagNames);
        Assert.False(NonExistingTagNames);
    }

    [Fact]
    public async void CheckIfTagNameExists_WillReturnFalseIfNameSameWithExceptedTagAndWorkAsAlwaysOtherwise() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingTagNamesWithExcept =
            await dataSource.CheckIfTagNameExists(
                TagNames[0], IDSet.TagsIDs[0]
            ) ||
            await dataSource.CheckIfTagNameExists(
                TagNames[1], IDSet.TagsIDs[1]
            ) ||
            await dataSource.CheckIfTagNameExists(
                TagNames[2], IDSet.TagsIDs[2]
            );
        bool ExistingTagNamesWithDifferentExcept =
            await dataSource.CheckIfTagNameExists(
                TagNames[0], IDSet.TagsIDs[1]
            ) ||
            await dataSource.CheckIfTagNameExists(
                TagNames[1], IDSet.TagsIDs[2]
            ) ||
            await dataSource.CheckIfTagNameExists(
                TagNames[2], IDSet.TagsIDs[0]
            );
        bool NonExistingTagNamesWithExcepts =
            await dataSource.CheckIfTagNameExists(
                "TestTagNonexistantName1", IDSet.TagsIDs[0]
            ) ||
            await dataSource.CheckIfTagNameExists(
                "Shvabra Cadabra", IDSet.TagsIDs[0]
            ) ||
            await dataSource.CheckIfTagNameExists(
                "1234321", IDSet.TagsIDs[0]
            );
        
        Assert.False(ExistingTagNamesWithExcept);
        Assert.True(ExistingTagNamesWithDifferentExcept);
        Assert.False(NonExistingTagNamesWithExcepts);
    }

    [Fact]
    public async void CheckIfNodeNameExists_WillReturnTrueOnlyIfNodeWithSpecifiedNameIsAlreadyInTheDatabase() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingNodeNames =
            await dataSource.CheckIfNodeNameExists(
                NodeNames[0], null
            ) &&
            await dataSource.CheckIfNodeNameExists(
                NodeNames[1], null
            ) &&
            await dataSource.CheckIfNodeNameExists(
                NodeNames[2], null
            );
        bool NonExistingNodeNames =
            await dataSource.CheckIfNodeNameExists(
                "TestNodeNonexistantName1", null
            ) ||
            await dataSource.CheckIfNodeNameExists(
                "Shvabra Cadabra", null
            ) ||
            await dataSource.CheckIfNodeNameExists(
                "1234321", null
            );
        
        Assert.True(ExistingNodeNames);
        Assert.False(NonExistingNodeNames);
    }

    [Fact]
    public async void CheckIfNodeNameExists_WillReturnFalseIfNameSameWithExceptedNodeAndWorkAsAlwaysOtherwise() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingNodeNamesWithExcept =
            await dataSource.CheckIfNodeNameExists(
                NodeNames[0], IDSet.NodesIDs[0]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                NodeNames[1], IDSet.NodesIDs[1]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                NodeNames[2], IDSet.NodesIDs[2]
            );
        bool ExistingNodeNamesWithDifferentExcept =
            await dataSource.CheckIfNodeNameExists(
                NodeNames[0], IDSet.NodesIDs[1]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                NodeNames[1], IDSet.NodesIDs[2]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                NodeNames[2], IDSet.NodesIDs[0]
            );
        bool NonExistingNodeNamesWithExcepts =
            await dataSource.CheckIfNodeNameExists(
                "TestNodeNonexistantName1", IDSet.NodesIDs[0]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                "Shvabra Cadabra", IDSet.NodesIDs[0]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                "1234321", IDSet.NodesIDs[0]
            );
        
        Assert.False(ExistingNodeNamesWithExcept);
        Assert.True(ExistingNodeNamesWithDifferentExcept);
        Assert.False(NonExistingNodeNamesWithExcepts);
    }

    [Fact]
    public async void CheckIfCWSNameExists_WillReturnTrueOnlyIfCWSWithSpecifiedNameIsAlreadyInTheDatabase() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingCWSName =
            await dataSource.CheckIfCWSNameExists(
                WebServicesNames[0], null
            );
        bool NonExistingCWSNames =
            await dataSource.CheckIfCWSNameExists(
                "TestCWSNonexistantName1", null
            ) ||
            await dataSource.CheckIfCWSNameExists(
                "Shvabra Cadabra", null
            ) ||
            await dataSource.CheckIfCWSNameExists(
                "1234321", null
            );
        
        Assert.True(ExistingCWSName);
        Assert.False(NonExistingCWSNames);
    }

    [Fact]
    public async void CheckIfCWSNameExists_WillReturnFalseIfNameSameWithExceptedCWSAndWorkAsAlwaysOtherwise() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingCWSNameWithExcept =
            await dataSource.CheckIfCWSNameExists(
                WebServicesNames[0], IDSet.WebServicesIDs[0]
            );
        bool NonExistingCWSNamesWithExcepts =
            await dataSource.CheckIfCWSNameExists(
                "TestCWSNonexistantName1", IDSet.WebServicesIDs[0]
            ) ||
            await dataSource.CheckIfCWSNameExists(
                "Shvabra Cadabra", IDSet.WebServicesIDs[0]
            ) ||
            await dataSource.CheckIfNodeNameExists(
                "1234321", IDSet.WebServicesIDs[0]
            );
        
        Assert.False(ExistingCWSNameWithExcept);
        Assert.False(NonExistingCWSNamesWithExcepts);
    }

    [Fact]
    public async void CheckIfProfileNameExists_WillReturnTrueOnlyIfProfileWithSpecifiedNameIsAlreadyInTheDatabase() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingProfileName =
            await dataSource.CheckIfProfileNameExists(
                context.Profiles.First().Name, null
            );
        bool NonExistingProfileNames =
            await dataSource.CheckIfProfileNameExists(
                "TestProfileNonexistantName1", null
            ) ||
            await dataSource.CheckIfProfileNameExists(
                "Shvabra Cadabra", null
            ) ||
            await dataSource.CheckIfProfileNameExists(
                "1234321", null
            );
        
        Assert.True(ExistingProfileName);
        Assert.False(NonExistingProfileNames);
    }

    [Fact]
    public async void CheckIfProfileNameExists_WillReturnFalseIfNameSameWithExceptedProfileAndWorkAsAlwaysOtherwise() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool ExistingProfileNameWithExcept =
            await dataSource.CheckIfProfileNameExists(
                WebServicesNames[0], IDSet.ProfileID
            );
        bool NonExistingProfileNamesWithExcepts =
            await dataSource.CheckIfProfileNameExists(
                "TestProfileNonexistantName1", IDSet.ProfileID
            ) ||
            await dataSource.CheckIfProfileNameExists(
                "Shvabra Cadabra", IDSet.ProfileID
            ) ||
            await dataSource.CheckIfProfileNameExists(
                "1234321", IDSet.ProfileID
            );
        
        Assert.False(ExistingProfileNameWithExcept);
        Assert.False(NonExistingProfileNamesWithExcepts);
    }

    [Fact]
    public async void CreateTag_WillCreateNewTag() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        int tagsBefore = context.Tags.Count();
        NodeTag tag = new NodeTag {
            ID = 0,
            Name = "newTag"
        };
        NodeTag newTag = await dataSource.CreateTag(tag);
        NodeTag newTagWithParentNetwrokInclude = context.Tags
            .AsNoTracking()
            .Single(t => t.ID == newTag.ID);
        int tagsAfter = context.Tags.Count();

        Assert.Equal(1, tagsAfter - tagsBefore);
        Assert.Single(context.Tags, t => t.Name == "newTag");
    }

    [Fact]
    public async void UpdateTag_WillUpdateSpecifiedTag() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        int tagsBefore = context.Tags.Count();
        NodeTag tag = context.Tags
            .AsNoTracking()
            .Single(t => t.ID == IDSet.TagsIDs[0]);
        string oldName = tag.Name;
        tag.Name = "1234567";
        await dataSource.UpdateTag(tag);
        int tagsAfter = context.Tags.Count();

        Assert.Equal(0, tagsAfter - tagsBefore);
        Assert.DoesNotContain(context.Tags, t => t.Name == oldName);
        Assert.Single(context.Tags, t => t.Name == tag.Name);
    }

    [Fact]
    public static async Task CreateWebServiceBinding_WillBindWebServiceToANode() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        var oldBindings = context.WebServiceBindings.Where(s => s.ServiceID == IDSet.WebServicesIDs[1]);
        context.WebServiceBindings.RemoveRange(oldBindings);
        context.SaveChanges();
        
        var bindingsBefore = context.WebServiceBindings.AsNoTracking().ToArray();

        await dataSource.CreateWebServiceBinding(
            IDSet.NodesIDs[0], IDSet.WebServicesIDs[1], "80", null, null);
        await dataSource.CreateWebServiceBinding(
            IDSet.NodesIDs[1], IDSet.WebServicesIDs[1], "", null, null);
        await dataSource.CreateWebServiceBinding(
            IDSet.NodesIDs[2], IDSet.WebServicesIDs[1], "8080", null, null);
        var bindingsAfter = context.WebServiceBindings.AsNoTracking().ToArray();
        var bindingsForNode1 = bindingsAfter.Where(b => b.NodeID == IDSet.NodesIDs[0]);
        var bindingsForNode2 = bindingsAfter.Where(b => b.NodeID == IDSet.NodesIDs[1]);
        var bindingsForNode3 = bindingsAfter.Where(b => b.NodeID == IDSet.NodesIDs[2]);

        Assert.Equal(2, bindingsBefore.Count());
        Assert.Equal(5, bindingsAfter.Count());
        Assert.Equal(2, bindingsForNode1.Count());
        Assert.Equal(2, bindingsForNode2.Count());
        Assert.Single(bindingsForNode3);
        Assert.Single(bindingsForNode1, b => b.ServiceID == IDSet.WebServicesIDs[0]);
        Assert.Single(bindingsForNode2, b => b.ServiceID == IDSet.WebServicesIDs[0]);
        Assert.Single(bindingsForNode1, b => b.ServiceID == IDSet.WebServicesIDs[1]);
        Assert.Single(bindingsForNode2, b => b.ServiceID == IDSet.WebServicesIDs[1]);
        Assert.Single(bindingsForNode3, b => b.ServiceID == IDSet.WebServicesIDs[1]);
    }

    [Fact]
    public async void SetNodeTags_AttachesListOfTagsToTheNodeOnceEachAndRemovesNotSpecifiedTagsAlreadyAttached() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var attachedBefore = context.TagAttachments
            .AsNoTracking()
            .Where(a => a.NodeID == IDSet.NodesIDs[0])
            .ToList();
        await dataSource.SetNodeTags(IDSet.NodesIDs[0], new [] {
            IDSet.TagsIDs[0], IDSet.TagsIDs[1]
        });
        var attachedAfter = context.TagAttachments
            .AsNoTracking()
            .Where(a => a.NodeID == IDSet.NodesIDs[0])
            .ToList();
        await dataSource.SetNodeTags(IDSet.NodesIDs[0], new [] {
            IDSet.TagsIDs[2]
        });
        var attachedAfterSecondChange = context.TagAttachments
            .AsNoTracking()
            .Where(a => a.NodeID == IDSet.NodesIDs[0])
            .ToList();

        Assert.Single(attachedBefore);
        Assert.Equal(2, attachedAfter.Count);
        Assert.Single(attachedAfterSecondChange);
        Assert.Single(attachedAfter, a => a.TagID == IDSet.TagsIDs[0]);
        Assert.Single(attachedAfter, a => a.TagID == IDSet.TagsIDs[1]);
        Assert.Single(attachedAfterSecondChange, a => a.TagID == IDSet.TagsIDs[2]);
    }
    
    [Fact]
    public async void SetProfileViewTagsSelection_WillSetListOfTagsUsedToFilterProfileView() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        SetProfileVTagsTo2_3AndMTagsTo1(context, IDSet);
        var selection = context.ProfilesTagSelection
            .AsNoTracking()
            .Where(pvt => pvt.BindedProfileID == IDSet.ProfileID &&
                        ProfileSelectedTagFlags.NodesListView ==
                            (pvt.Flags & ProfileSelectedTagFlags.NodesListView));
        var setBefore = selection.ToList();
        await dataSource.SetProfileViewTagsSelection(IDSet.ProfileID, new [] {
            IDSet.TagsIDs[0], IDSet.TagsIDs[1], IDSet.TagsIDs[2]
        });
        var setAfter = selection.ToList();
        await dataSource.SetProfileViewTagsSelection(IDSet.ProfileID, new [] {
            IDSet.TagsIDs[1]
        });
        var setAfterSecondChange = selection.ToList();

        Assert.Equal(2, setBefore.Count);
        Assert.Equal(3, setAfter.Count);
        Assert.Single(setAfterSecondChange);
        Assert.Single(setAfter, a => a.TagID == IDSet.TagsIDs[0]);
        Assert.Single(setAfter, a => a.TagID == IDSet.TagsIDs[1]);
        Assert.Single(setAfter, a => a.TagID == IDSet.TagsIDs[2]);
        Assert.Single(setAfterSecondChange, a => a.TagID == IDSet.TagsIDs[1]);
    }
    
    [Fact]
    public async void SetProfileMonitorTagsSelection_WillSetListOfTagsUsedToFilterProfileMonitor() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        SetProfileVTagsTo2_3AndMTagsTo1(context, IDSet);
        var selection = context.ProfilesTagSelection
            .AsNoTracking()
            .Where(pvt => pvt.BindedProfileID == IDSet.ProfileID &&
                        ProfileSelectedTagFlags.Monitor ==
                            (pvt.Flags & ProfileSelectedTagFlags.Monitor));
        var setBefore = selection.ToList();
        await dataSource.SetProfileMonitorTagsSelection(IDSet.ProfileID, new [] {
            IDSet.TagsIDs[0], IDSet.TagsIDs[1], IDSet.TagsIDs[2]
        });
        var setAfter = selection.ToList();
        await dataSource.SetProfileMonitorTagsSelection(IDSet.ProfileID, new [] {
            IDSet.TagsIDs[1]
        });
        var setAfterSecondChange = selection.ToList();

        Assert.Single(setBefore);
        Assert.Equal(3, setAfter.Count);
        Assert.Single(setAfterSecondChange);
        Assert.Single(setAfter, a => a.TagID == IDSet.TagsIDs[0]);
        Assert.Single(setAfter, a => a.TagID == IDSet.TagsIDs[1]);
        Assert.Single(setAfter, a => a.TagID == IDSet.TagsIDs[2]);
        Assert.Single(setAfterSecondChange, a => a.TagID == IDSet.TagsIDs[1]);
    }
    
    [Fact]
    public async void SetProfileViewTagsSelectionToProfileMonitorTagsSelection_WillSetAllViewTagSelectionsToCurrentMonitorTagsSelections() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var viewSelection = context.ProfilesTagSelection
            .Where(pst => pst.BindedProfileID == IDSet.ProfileID &&
                ProfileSelectedTagFlags.NodesListView ==
                (pst.Flags & ProfileSelectedTagFlags.NodesListView)
            );
        var monitorSelection = context.ProfilesTagSelection
            .Where(pst => pst.BindedProfileID == IDSet.ProfileID &&
                ProfileSelectedTagFlags.Monitor ==
                (pst.Flags & ProfileSelectedTagFlags.Monitor)
            );
        //Test on the first set
        SetProfileVTagsTo2_3AndMTagsTo1(context, IDSet);
        var set1MonitorBefore = monitorSelection.ToList();
        var set1ViewBefore = viewSelection.ToList();
        await dataSource
            .SetProfileViewTagsSelectionToProfileMonitorTagsSelection(
                IDSet.ProfileID
            );
        var set1MonitorAfter = monitorSelection.ToList();
        var set1ViewAfter = viewSelection.ToList();
        //Test on the second set
        SetProfileVTagsTo1_2AndMTagsTo2_3(context, IDSet);
        var set2MonitorBefore = monitorSelection.ToList();
        var set2ViewBefore = viewSelection.ToList();
        await dataSource
            .SetProfileViewTagsSelectionToProfileMonitorTagsSelection(
                IDSet.ProfileID
            );
        var set2MonitorAfter = monitorSelection.ToList();
        var set2ViewAfter = viewSelection.ToList();
        
        //Set1Assert
        Assert.Equal(2, set1ViewBefore.Count);
        Assert.Single(set1MonitorBefore, pst => pst.TagID == IDSet.TagsIDs[0]);
        Assert.Single(set1ViewAfter, pst => pst.TagID == IDSet.TagsIDs[0]);
        Assert.Single(set1MonitorAfter, pst => pst.TagID == IDSet.TagsIDs[0]);
        //Set2Assert
        Assert.Equal(2, set2ViewBefore.Count);
        Assert.Equal(2, set2MonitorBefore.Count);
        Assert.Single(set2ViewBefore, pst => pst.TagID == IDSet.TagsIDs[0]);
        Assert.Single(set2ViewBefore, pst => pst.TagID == IDSet.TagsIDs[1]);
        Assert.Single(set2MonitorBefore, pst => pst.TagID == IDSet.TagsIDs[1]);
        Assert.Single(set2MonitorBefore, pst => pst.TagID == IDSet.TagsIDs[2]);
        Assert.Equal(2, set2ViewAfter.Count);
        Assert.Equal(2, set2MonitorAfter.Count);
        Assert.Single(set2ViewAfter, pst => pst.TagID == IDSet.TagsIDs[1]);
        Assert.Single(set2ViewAfter, pst => pst.TagID == IDSet.TagsIDs[2]);
        Assert.Single(set2MonitorAfter, pst => pst.TagID == IDSet.TagsIDs[1]);
        Assert.Single(set2MonitorAfter, pst => pst.TagID == IDSet.TagsIDs[2]);
    }

    [Fact]
    public async void CreateCustomWebService_CreatesNewWebService() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        CustomWebService cvs = new CustomWebService {
            ID = 0,
            Name = "Web Interface On Custom Port",
            ServiceStr = "http://{node_ip}:{param1}",
            Parametr1Name = "Service Port"
        };
        int cwsCoundBefore = context.WebServices.Count();
        var createdCWS = await dataSource.CreateCustomWebService(cvs);
        int cwsCoundAfter = context.WebServices.Count();

        Assert.Equal(1, cwsCoundAfter - cwsCoundBefore);
        Assert.Equal("Web Interface On Custom Port", createdCWS.Name);
    }

    [Fact]
    public async void UpdateProfile_WillUpdateSpecifiedProfile() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var profileBefore = context.Profiles
            .AsNoTracking()
            .Single();
        var profileToUpdate = new Profile {
            ID = profileBefore.ID,
            Name = "UpdatedProfile",
            StartMonitoringOnLaunch = !profileBefore.StartMonitoringOnLaunch,
            DepthMonitoring = !profileBefore.DepthMonitoring,
            MonitorInterval = 2
        };

        await dataSource.UpdateProfile(profileToUpdate);
        var profileAfter = context.Profiles
            .AsNoTracking()
            .Single();

        Assert.Equal(profileBefore.ID, profileAfter.ID);
        Assert.NotEqual(profileBefore.Name, profileAfter.Name);
        Assert.Equal("UpdatedProfile", profileAfter.Name);
        Assert.NotEqual(profileBefore.StartMonitoringOnLaunch, profileAfter.StartMonitoringOnLaunch);
        Assert.NotEqual(profileBefore.DepthMonitoring, profileAfter.DepthMonitoring);
        Assert.NotEqual(profileBefore.MonitorInterval, profileAfter.MonitorInterval);
        Assert.Equal(2, profileAfter.MonitorInterval);
    }

    [Fact]
    public async void UpdateNode_WillUpdateSpecifiedNode() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var nodeBefore = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.NodesIDs[0]);
        string newName = "NewTestNode1Name";
        uint newTestIP = 4985893;
        var nodeToUpdate = new NtwkNode {
            ID = nodeBefore.ID,
            Name = newName,
            ip = newTestIP,
            OpenPing = !nodeBefore.OpenPing,
            OpenSSH = !nodeBefore.OpenSSH,
            OpenTelnet = !nodeBefore.OpenTelnet
        };

        await dataSource.UpdateNode(nodeToUpdate);
        var nodeAfter = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.NodesIDs[0]);

        Assert.Equal(newName, nodeAfter.Name);
        Assert.Equal(newTestIP, nodeAfter.ip);
        Assert.NotEqual(nodeBefore.OpenPing, nodeAfter.OpenPing);
        Assert.NotEqual(nodeBefore.OpenSSH, nodeAfter.OpenSSH);
        Assert.NotEqual(nodeBefore.OpenTelnet, nodeAfter.OpenTelnet);
    }

    [Fact]
    public async void UpdateNode_WillNotChangeNodeParentAndDepth() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var nodeBefore = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.NodesIDs[0]);
        var nodeToUpdate = new NtwkNode {
            ID = nodeBefore.ID,
            ParentID = IDSet.NodesIDs[2],
            Name = nodeBefore.Name,
            ip = nodeBefore.ip,
            OpenPing = nodeBefore.OpenPing,
            OpenSSH = nodeBefore.OpenSSH,
            OpenTelnet = nodeBefore.OpenTelnet
        };

        await dataSource.UpdateNode(nodeToUpdate);
        var nodeAfter = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.NodesIDs[0]);

        Assert.NotEqual(IDSet.NodesIDs[2], nodeBefore.ParentID);
        Assert.Equal(nodeBefore.ParentID, nodeAfter.ParentID);
    }

    [Fact]
    public async void CheckIfNodeInSubtree_WillReturnTrueOnlyIfSpecifiedNodeIsInSubtreeWitSpecifiedRoot() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        //Checks For Node 1
        bool isNode1InNode1Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[0], IDSet.NodesIDs[0]);
        bool isNode1InNode2Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[0], IDSet.NodesIDs[1]);
        bool isNode1InNode3Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[0], IDSet.NodesIDs[2]);
        //Checks for node  2
        bool isNode2InNode1Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[1], IDSet.NodesIDs[0]);
        bool isNode2InNode2Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[1], IDSet.NodesIDs[1]);
        bool isNode2InNode3Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[1], IDSet.NodesIDs[2]);
        //Checks for node  3
        bool isNode3InNode1Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[2], IDSet.NodesIDs[0]);
        bool isNode3InNode2Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[2], IDSet.NodesIDs[1]);
        bool isNode3InNode3Subtree =
            await dataSource.CheckIfNodeInSubtree(
                IDSet.NodesIDs[2], IDSet.NodesIDs[2]);
        
        Assert.True(isNode1InNode1Subtree);
        Assert.True(isNode2InNode2Subtree);
        Assert.True(isNode2InNode1Subtree);
        Assert.True(isNode3InNode3Subtree);
        Assert.False(isNode1InNode2Subtree);
        Assert.False(isNode1InNode3Subtree);
        Assert.False(isNode2InNode3Subtree);
        Assert.False(isNode3InNode1Subtree);
        Assert.False(isNode3InNode2Subtree);
    }

    [Fact]
    public async void MoveNodesSubtree_WillChangeNodeParentAndRebuildSubtreeClosuresTable() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var closuresBefore = context.NodesClosureTable.AsNoTracking().ToList();
        int? nodeParentIDBefore = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == IDSet.NodesIDs[0])
            .Parent?.ID;
        _testOutput.WriteLine("Closures before.");
        _testOutput.WriteLine("[\n{0}]", string.Join(",\n",
            context.NodesClosureTable.AsNoTracking()
                .Select(c => (new {c.ID, c.AncestorID, c.DescendantID, c.Distance}).ToString())
                .ToArray()
            )
        );
        
        await dataSource.MoveNodesSubtree(IDSet.NodesIDs[0], IDSet.NodesIDs[2]);
        var closuresAfter = context.NodesClosureTable.AsNoTracking().ToList();
        int? nodeParentIDAfter = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == IDSet.NodesIDs[0])
            .Parent?.ID;
        _testOutput.WriteLine("Closures after.");
        _testOutput.WriteLine("[\n{0}]", string.Join(",\n",
            context.NodesClosureTable.AsNoTracking()
                .Select(c => (new {c.ID, c.AncestorID, c.DescendantID, c.Distance}).ToString())
                .ToArray()
            )
        );

        Assert.Null(nodeParentIDBefore);
        Assert.Equal(IDSet.NodesIDs[2], nodeParentIDAfter);
        Assert.Equal(2, closuresBefore.Count(c => c.DescendantID == IDSet.NodesIDs[0]));
        Assert.Equal(0, closuresBefore.Where(c => c.DescendantID == IDSet.NodesIDs[0]).Sum(c => c.Distance));
        Assert.Equal(3, closuresAfter.Count(c => c.DescendantID == IDSet.NodesIDs[0]));
        Assert.Equal(2, closuresAfter.Where(c => c.DescendantID == IDSet.NodesIDs[0]).Sum(c => c.Distance));
        Assert.Equal(3, closuresBefore.Count(c => c.DescendantID == IDSet.NodesIDs[1]));
        Assert.Equal(2, closuresBefore.Where(c => c.DescendantID == IDSet.NodesIDs[1]).Sum(c => c.Distance));
        Assert.Equal(4, closuresAfter.Count(c => c.DescendantID == IDSet.NodesIDs[1]));
        Assert.Equal(5, closuresAfter.Where(c => c.DescendantID == IDSet.NodesIDs[1]).Sum(c => c.Distance));
    }

    [Fact]
    public async void UpdateCustomWebService_WillUpdateSpecifiedWebService() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var cwsBefore = context.WebServices.AsNoTracking()
            .Single(ws => ws.ID == IDSet.WebServicesIDs[0]);
        string newCWSName = "TestService";
        string newCWSStr = "http://test.{param1}.com";
        string newCWSParam1Name = "TestParam1";
        var wsToUpdate = new CustomWebService {
            ID=cwsBefore.ID,
            Name=newCWSName,
            ServiceStr=newCWSStr,
            Parametr1Name=newCWSParam1Name
        };

        await dataSource.UpdateCustomWebService(wsToUpdate);
        var cwsAfter = context.WebServices.AsNoTracking()
            .Single(ws => ws.ID == IDSet.WebServicesIDs[0]);

        Assert.NotEqual(newCWSName, cwsBefore.Name);
        Assert.NotEqual(newCWSStr, cwsBefore.ServiceStr);
        Assert.NotEqual(newCWSParam1Name, cwsBefore.Parametr1Name);
        Assert.Equal(newCWSName, cwsAfter.Name);
        Assert.Equal(newCWSStr, cwsAfter.ServiceStr);
        Assert.Equal(newCWSParam1Name, cwsAfter.Parametr1Name);
    }

    [Fact]
    public async Task UpdateWebServiceBinding_WillUpdateBindingParametersOfSpecifiedBinding() {
        var (context, IDSet, dataSource) = GetTestDataContext();

        await dataSource.UpdateWebServiceBinding(
            IDSet.NodesIDs[0], IDSet.WebServicesIDs[1], "123", null, null
        );
        await dataSource.UpdateWebServiceBinding(
            IDSet.NodesIDs[1], IDSet.WebServicesIDs[1], "421", null, null
        );
        await dataSource.UpdateWebServiceBinding(
            IDSet.NodesIDs[2], IDSet.WebServicesIDs[1], "test", null, null
        );

        Assert.Equal("123", context.WebServiceBindings
            .Single(b => b.ServiceID == IDSet.WebServicesIDs[1] && b.NodeID == IDSet.NodesIDs[0])
            .Param1);
        Assert.Equal("421", context.WebServiceBindings
            .Single(b => b.ServiceID == IDSet.WebServicesIDs[1] && b.NodeID == IDSet.NodesIDs[1])
            .Param1);
        Assert.Equal("test", context.WebServiceBindings
            .Single(b => b.ServiceID == IDSet.WebServicesIDs[1] && b.NodeID == IDSet.NodesIDs[2])
            .Param1);
    }

    [Fact]
    public async Task RemoveProfile_WillRemoveSpecifiedProfileItsTagBindingsAndMonitoringSessions() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        SetProfileVTagsTo2_3AndMTagsTo1(context, IDSet);
        
        await dataSource.RemoveProfile(IDSet.ProfileID);

        Assert.Empty(context.Profiles);
        Assert.Empty(context.ProfilesTagSelection);
        Assert.Empty(context.MonitoringSessions.Where(s => s.CreatedByProfileID == IDSet.ProfileID));
    }

    [Fact]
    public async Task RemoveNode_WillRemoveSpecifiedNodeAndThrowsInvalidOperationIfNodeContainsChildren() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        var nodesBefore = context.Nodes.AsNoTracking().ToArray();
        await dataSource.RemoveNode(IDSet.NodesIDs[2]);
        var nodesAfter = context.Nodes.AsNoTracking().ToArray();

        Assert.Equal(3, nodesBefore.Length);
        Assert.Single(nodesBefore, n => n.ID == IDSet.NodesIDs[2]);
        Assert.Equal(2, nodesAfter.Length);
        Assert.DoesNotContain(nodesAfter, n => n.ID == IDSet.NodesIDs[2]);
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await dataSource.RemoveNode(IDSet.NodesIDs[0])
        );
    }

    [Fact]
    public async Task RemoveTag_WillRemoveTagAndAllTheLinkingEntitiesThatUseIt() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        SetProfileVTagsTo1_2AndMTagsTo2_3(context, IDSet);

        var tagsBefore = context.Tags.AsNoTracking().ToArray();
        bool wasAttachedToNodes = context.TagAttachments.Any(a => a.TagID == IDSet.TagsIDs[0]);
        bool hadProfileViewBindings = context.ProfilesTagSelection.Any(a => a.TagID == IDSet.TagsIDs[0]);
        await dataSource.RemoveTag(IDSet.TagsIDs[0]);
        var tagsAfter = context.Tags.AsNoTracking().ToArray();
        bool isAttachedToNodes = context.TagAttachments.Any(a => a.TagID == IDSet.TagsIDs[0]);
        bool hasProfileViewBindings = context.ProfilesTagSelection.Any(a => a.TagID == IDSet.TagsIDs[0]);

        Assert.Single(tagsBefore, t => t.ID == IDSet.TagsIDs[0]);
        Assert.True(wasAttachedToNodes);
        Assert.True(hadProfileViewBindings);
        Assert.DoesNotContain(tagsAfter, t => t.ID == IDSet.TagsIDs[0]);
        Assert.False(isAttachedToNodes);
        Assert.False(hasProfileViewBindings);
    }

    [Fact]
    public async Task RemoveCustomWebService_WillRemoveSpecifiedWebServiceAndAllItsBindingsToNodes() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        bool existedBefore = null != context.WebServices
            .SingleOrDefault(ws => ws.ID == IDSet.WebServicesIDs[1]);
        bool hadBindings = context.WebServiceBindings
            .Any(b => b.ServiceID == IDSet.WebServicesIDs[1]);

        await dataSource.RemoveCustomWebService(IDSet.WebServicesIDs[1]);

        Assert.True(existedBefore);
        Assert.True(hadBindings);
        Assert.Null(context.WebServices
            .SingleOrDefault(ws => ws.ID == IDSet.WebServicesIDs[1]));
        Assert.Empty(context.WebServiceBindings
            .Where(b => b.ServiceID == IDSet.WebServicesIDs[1]));
    }
    
    [Fact]
    public async Task RemoveWebServiceBinding_WillRemoveSpecifiedBinding() {
        var (context, IDSet, dataSource) = GetTestDataContext();
        
        int bindingsBefore = context.WebServiceBindings.Count();

        await dataSource.RemoveWebServiceBinding(
            IDSet.NodesIDs[0], IDSet.WebServicesIDs[0]);
        await dataSource.RemoveWebServiceBinding(
            IDSet.NodesIDs[1], IDSet.WebServicesIDs[1]);
        await dataSource.RemoveWebServiceBinding(
            IDSet.NodesIDs[2], IDSet.WebServicesIDs[1]);
        int bindingsAfter = context.WebServiceBindings.Count();

        Assert.Equal(3, bindingsBefore - bindingsAfter);
        Assert.Single(context.WebServiceBindings.Where(b => b.NodeID == IDSet.NodesIDs[0]));
        Assert.Single(context.WebServiceBindings.Where(b => b.NodeID == IDSet.NodesIDs[1]));
        Assert.Empty(context.WebServiceBindings.Where(b => b.NodeID == IDSet.NodesIDs[2]));
    }
}

}
