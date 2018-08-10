using System;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System.Linq;
using System.Threading.Tasks;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.EFDatabase;
using Data.EFDatabase.Logic;

namespace Tests {

public class Data_EFDataSourceLogicTest {

    private readonly ITestOutputHelper testOutput;

    public Data_EFDataSourceLogicTest(ITestOutputHelper output) {
        this.testOutput = output;
    }

    [Fact]
    public async Task GetNewSession_CreatesNewSession() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.SetProfileViewTagsTo1And2AndMonitorTagsTo2And3(context, IDSet);

        var createdSession = await EFDataSourceLogic.GetNewSession_Logic(context, IDSet.ProfileID);
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var pulse = new MonitoringPulseResult {
            ID = 0,
            Responded = 2,
            Silent = 0,
            Skipped = 0,
        };
        var message = new MonitoringMessage {
            ID = 0,
            MessageType = MonitoringMessageType.Warning_InconsistentPing,
            MessageSourceNodeName = utils.FirstNodeName
        };
        var messages = new MonitoringMessage[1];
        messages[0] = message;
        int sessionID = context.MonitoringSessions
            .AsNoTracking()
            .First()
            .ID;

        int createdPulseID = (await EFDataSourceLogic.SavePulseResult_Logic(
            context, sessionID,
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
        Assert.Equal(utils.FirstNodeName, createdPulse.Messages.Single().MessageSourceNodeName);
    }

    [Fact]
    public async void ClearEmptySessionsLogic_RemovedAllEmptySessionsOnly() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
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
            .Single(t => t.ID == IDSet.Tag1ID);
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
            .Where(s => s.Pulses.Count == 0)
            .Count();
        await EFDataSourceLogic.ClearEmptySessions_Logic(context);
        var countSessionsAfter = context.MonitoringSessions.Count();
        var countEmptySessionsAfter = context.MonitoringSessions
            .AsNoTracking()
            .Include(s => s.Pulses)
            .Where(s => s.Pulses.Count == 0)
            .Count();
        
        Assert.Equal(countSessionsBefore-countEmptySessionsBefore,
            countSessionsAfter);
        Assert.Equal(0, countEmptySessionsAfter);
    }

    [Fact]
    public async void GetSessionForProfileLogic_GetsAllSessionsForProfile() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var testProfile2 = new Profile {
            ID = 0,
            Name = "TestProfile2",
            StartMonitoringOnLaunch = true,
            DepthMonitoring = true,
            MonitorInterval = 10
        };
        context.Profiles.Add(testProfile2);
        context.SaveChanges();

        var sessionsForProfile1 = await EFDataSourceLogic.GetSessionsForProfile_Logic(context, IDSet.ProfileID);
        var sessionsForProfile2 = await EFDataSourceLogic.GetSessionsForProfile_Logic(context, testProfile2.ID);

        Assert.Single(sessionsForProfile1);
        Assert.Empty(sessionsForProfile2);
    }

    [Fact]
    public async void GetSessionReport_GetsAllPulsesForSession() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        int sessionID = context.MonitoringSessions
            .AsNoTracking()
            .Single(s => s.CreationTime == 1528359015)
            .ID;
        var pulsesForSession1 = await EFDataSourceLogic.GetSessionReport_Logic(context, sessionID);

        Assert.Equal(2, pulsesForSession1.Count());
        Assert.Contains(pulsesForSession1, p => p.CreationTime == 1528359015);
        Assert.Contains(pulsesForSession1, p => p.CreationTime == 1528360285);
    }

    [Fact]
    public async void GetAllProfiles_GetsAllProfilesInDB() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var testProfile2 = new Profile {
            ID = 0,
            Name = "TestProfile2",
            StartMonitoringOnLaunch = true,
            DepthMonitoring = true,
            MonitorInterval = 10
        };
        context.Profiles.Add(testProfile2);
        context.SaveChanges();

        var profiles = await EFDataSourceLogic.GetAllProfiles_Logic(context);

        Assert.Equal(2, profiles.Count());
    }

    [Fact]
    public async void GetAllNodesData_GetsAllNodesDataObjectWithNodesGroupedByTreeDepthWithBoundServicesIDsAndTagIDs() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.CreateClosuresForTestNodes(context, IDSet);
        var cwData = context.WebServices.AsNoTracking()
            .Select(ws => new { ws.ID, ws.ServiceName })
            .ToList();

        Data.Model.IntermediateModel.AllRawNodesData allNodesData = 
            await EFDataSourceLogic.GetAllNodesData_Logic(context);
    
        //Assert nodes groups structure
        Assert.Equal(2, allNodesData.NodesData.Count());
        Assert.Equal(2, allNodesData.NodesData.First().Count());
        Assert.Equal(1, allNodesData.NodesData.Skip(1).First().Count());
        Assert.NotEqual(null, allNodesData.NodesData.First().SingleOrDefault(nd => nd.Node.ID == IDSet.Node1ID));
        Assert.NotEqual(null, allNodesData.NodesData.First().SingleOrDefault(nd => nd.Node.ID == IDSet.Node3ID));
        Assert.NotEqual(null, allNodesData.NodesData.Skip(1).First().SingleOrDefault(nd => nd.Node.ID == IDSet.Node2ID));
        //Assert ws data
        Assert.All(
            allNodesData.WebServicesData.Zip(cwData,  (o1, o2) => new { o1, o2 }),
            (o) => {
                if(o.o1.ID != o.o2.ID) {
                    throw new Exception($"Unexpected ID value: {o.o1.ID} != {o.o2.ID}");
                }
                if(o.o1.Name != o.o2.ServiceName) {
                    throw new Exception($"Unexpected Name value: {o.o1.Name} != {o.o2.ServiceName}");
                }
            }
        );
    }

    [Fact]
    public async void GetNodeIP_GetsNodeIPFromDB() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        var ip = await EFDataSourceLogic.GetNodeIP_Logic(context, IDSet.Node1ID);

        Assert.Equal(context.Nodes.Single(n => n.ID == IDSet.Node1ID).ip, ip);
    }

    [Fact]
    public async void GetTaggedNodesIDs_GetsOnlyAllTheNodesIDsForNodesWithSpecificTag() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        
        var tag1Nodes = await EFDataSourceLogic.GetTaggedNodesIDs_Logic(context, IDSet.Tag1ID);
        var tag2Nodes = await EFDataSourceLogic.GetTaggedNodesIDs_Logic(context, IDSet.Tag2ID);
        var tag3Nodes = await EFDataSourceLogic.GetTaggedNodesIDs_Logic(context, IDSet.Tag3ID);

        Assert.Equal(2, tag1Nodes.Count());
        Assert.Single(tag2Nodes);
        Assert.Single(tag3Nodes);
        Assert.Contains(IDSet.Node1ID, tag1Nodes);
        Assert.Contains(IDSet.Node2ID, tag1Nodes);
        Assert.Contains(IDSet.Node2ID, tag2Nodes);
        Assert.Contains(IDSet.Node3ID, tag3Nodes);
    }

    [Fact]
    public async void GetIDsOfNodesBySelectedTagsInProfileView_WillGetAllNodesWithTagsSpecifiedInProfileView() {   
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        utils.SetProfileViewTagsTo1And2AndMonitorTagsTo2And3(context, IDSet);
        var viewNodesForTags1And2 = await EFDataSourceLogic
            .GetIDsOfNodesBySelectedTagsInProfileView_Logic(context, IDSet.ProfileID);
        utils.SetProfileViewTagsTo2And3AndMonitorTagsTo1(context, IDSet);
        var viewNodesForTags2And3 = await EFDataSourceLogic
            .GetIDsOfNodesBySelectedTagsInProfileView_Logic(context, IDSet.ProfileID);

        Assert.Equal(2, viewNodesForTags1And2.Count());
        Assert.Single(viewNodesForTags1And2, IDSet.Node1ID);
        Assert.Single(viewNodesForTags1And2, IDSet.Node2ID);
        Assert.Equal(2, viewNodesForTags2And3.Count());
        Assert.Single(viewNodesForTags2And3, IDSet.Node2ID);
        Assert.Single(viewNodesForTags2And3, IDSet.Node3ID);
    }

    [Fact]
    public async void GetIDsOfNodesBySelectedTagsInProfileMonitor_WillGetAllNodesWithTagsSpecifiedInProfileMonitor() {   
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        utils.SetProfileViewTagsTo1And2AndMonitorTagsTo2And3(context, IDSet);
        var monitorNodesForTags2And3 = await EFDataSourceLogic
            .GetIDsOfNodesBySelectedTagsInProfileMonitor_Logic(context, IDSet.ProfileID);
        utils.SetProfileViewTagsTo2And3AndMonitorTagsTo1(context, IDSet);
        var monitorNodesForTags1 = await EFDataSourceLogic
            .GetIDsOfNodesBySelectedTagsInProfileMonitor_Logic(context, IDSet.ProfileID);

        Assert.Equal(2, monitorNodesForTags2And3.Count());
        Assert.Single(monitorNodesForTags2And3, IDSet.Node2ID);
        Assert.Single(monitorNodesForTags2And3, IDSet.Node3ID);
        Assert.Equal(2, monitorNodesForTags1.Count());
        Assert.Single(monitorNodesForTags1, IDSet.Node1ID);
        Assert.Single(monitorNodesForTags1, IDSet.Node2ID);
    }

    [Fact]
    public async void GetAllTags_ReturnsAllTheTags() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        var tags = await EFDataSourceLogic.GetAllTags_Logic(context);

        Assert.Equal(3, tags.Count());
    }

    [Fact]
    public async void GetAllCVS_ReturnsAllCVS() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        var cvsList = await EFDataSourceLogic.GetAllCWS_Logic(context);

        Assert.Single(cvsList);
        Assert.Equal(utils.WebInterfaceOn8080Name, cvsList.First().ServiceName);
    }

    [Fact]
    public async void CreateProfile_WillCreateNewProfile() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        
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
        var net1 = await EFDataSourceLogic.CreateProfile_Logic(context, newProfile);
        newProfile.Name = newProfile2Name;
        var net2 = await EFDataSourceLogic.CreateProfile_Logic(context, newProfile);
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var expectedClosuresForNode1 = new [] {
            new { ancestorID = (int?)null,          descendantID = IDSet.Node1ID, dist = 0 },
            new { ancestorID = (int?)IDSet.Node1ID, descendantID = IDSet.Node1ID, dist = 0 }
        };
        var expectedClosuresForNode2 = new [] {
            new { ancestorID = (int?)null,          descendantID = IDSet.Node2ID, dist = 1 },
            new { ancestorID = (int?)IDSet.Node1ID, descendantID = IDSet.Node2ID, dist = 1 },
            new { ancestorID = (int?)IDSet.Node2ID, descendantID = IDSet.Node2ID, dist = 0 }
        };
        var expectedClosuresForNode3 = new [] {
            new { ancestorID = (int?)null,          descendantID = IDSet.Node3ID, dist = 0 },
            new { ancestorID = (int?)IDSet.Node3ID, descendantID = IDSet.Node3ID, dist = 0 }
        };

        var clN1 = await EFDataSourceLogic.__CreateNewNodeClosures(context, null, IDSet.Node1ID);
        context.AddRange(clN1);
        context.SaveChanges();
        var clN2 = await EFDataSourceLogic.__CreateNewNodeClosures(context, IDSet.Node1ID, IDSet.Node2ID);
        context.AddRange(clN2);
        context.SaveChanges();
        var clN3 = await EFDataSourceLogic.__CreateNewNodeClosures(context, null, IDSet.Node3ID);
        context.AddRange(clN3);
        context.SaveChanges();
        var closuresForNode1 = context.NodesClosureTable
            .AsNoTracking()
            .Where(c => c.DescendantID == IDSet.Node1ID)
            .ToList();
        var closuresForNode2 = context.NodesClosureTable
            .AsNoTracking()
            .Where(c => c.DescendantID == IDSet.Node2ID)
            .ToList();
        var closuresForNode3 = context.NodesClosureTable
            .AsNoTracking()
            .Where(c => c.DescendantID == IDSet.Node3ID)
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

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
        var createdNode1 = await EFDataSourceLogic.CreateNodeOnRoot_Logic(
            context, newNode
        );
        newNode.Name = "NewNode2";
        var createdNode2 = await EFDataSourceLogic.CreateNodeOnRoot_Logic(
            context, newNode
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.CreateClosuresForTestNodes(context, IDSet);
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
        var createdNode1 = await EFDataSourceLogic.CreateNodeWithParent_Logic(
            context, newNode, IDSet.Node2ID
        );
        newNode.Name = "NewNode2";
        var createdNode2 = await EFDataSourceLogic.CreateNodeWithParent_Logic(
            context, newNode, IDSet.Node3ID
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
                ID=0, AncestorID = IDSet.Node1ID, DescendantID = createdNode1.ID, Distance = 2
            },
            new NodeClosure {
                ID=0, AncestorID = IDSet.Node2ID, DescendantID = createdNode1.ID, Distance = 1
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
                ID=0, AncestorID = IDSet.Node3ID, DescendantID = createdNode2.ID, Distance = 1
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
        Assert.Equal(IDSet.Node2ID, createdNode1WithParent.Parent.ID);
        Assert.Equal(IDSet.Node3ID, createdNode2WithParent.Parent.ID);
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.CreateClosuresForTestNodes(context, IDSet);
        
        bool node1HasChildren = await EFDataSourceLogic.HasChildren_Logic(
            context, IDSet.Node1ID
        );
        bool node2HasChildren = await EFDataSourceLogic.HasChildren_Logic(
            context, IDSet.Node2ID
        );
        bool node3HasChildren = await EFDataSourceLogic.HasChildren_Logic(
            context, IDSet.Node3ID
        );

        Assert.True(node1HasChildren);
        Assert.False(node2HasChildren);
        Assert.False(node3HasChildren);
    }

    [Fact]
    public async void CheckIfProfileExists_WillReturnTrueOnlyIfSpecifiedProfileExists() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool existingProfile = await EFDataSourceLogic
            .CheckIfProfileExists_Logic(context, IDSet.ProfileID);
        bool nonExistingProfile = await EFDataSourceLogic
            .CheckIfProfileExists_Logic(context, IDSet.ProfileID+199);
        
        Assert.True(existingProfile);
        Assert.False(nonExistingProfile);
    }

    [Fact]
    public async void CheckIfCWSExists_WillReturnTrueOnlyIfSpecifiedCWSExists() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        int existingCWS = await EFDataSourceLogic
            .GetCWSParamNumber_Logic(context, IDSet.WebServiceID);
        int nonExistingCWS = await EFDataSourceLogic
            .GetCWSParamNumber_Logic(context, IDSet.WebServiceID+199);
        
        Assert.Equal(0, existingCWS);
        Assert.Equal(-1, nonExistingCWS);
    }

    [Fact]
    public async void CheckIfSessionExists_WillReturnTrueOnlyIfSpecifiedMonitoringSessionExists() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        int ExistingSessionID = context.MonitoringSessions.First().ID;

        bool existingSession = await EFDataSourceLogic
            .CheckIfSessionExists_Logic(context, ExistingSessionID);
        bool nonExistingSession = await EFDataSourceLogic
            .CheckIfSessionExists_Logic(context, ExistingSessionID+199);
        
        Assert.True(existingSession);
        Assert.False(nonExistingSession);
    }

    [Fact]
    public async void CheckIfTagExists_WillReturnTrueOnlyIfSpecifiedTagExists() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool existingTags =
            await EFDataSourceLogic.CheckIfTagExists_Logic(
                context, IDSet.Tag1ID
            ) &&
            await EFDataSourceLogic.CheckIfTagExists_Logic(
                context, IDSet.Tag2ID
            ) &&
            await EFDataSourceLogic.CheckIfTagExists_Logic(
                context, IDSet.Tag3ID
            );
        bool nonExistingTags =
            await EFDataSourceLogic.CheckIfTagExists_Logic(
                context, IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID
            ) ||
            await EFDataSourceLogic.CheckIfTagExists_Logic(
                context, IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID+1
            ) ||
            await EFDataSourceLogic.CheckIfTagExists_Logic(
                context, IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID+2
            );
        
        Assert.True(existingTags);
        Assert.False(nonExistingTags);
    }

    

    [Fact]
    public async void CheckIfTagsExist_WillReturnTrueOnlyIfAllTheSpecifiedTagsExist() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool existingTags =
            await EFDataSourceLogic.CheckIfTagsExist_Logic(
                context, new [] { IDSet.Tag1ID , IDSet.Tag2ID, IDSet.Tag3ID}
            );
        bool nonExistingTags =
            await EFDataSourceLogic.CheckIfTagsExist_Logic(
                context,
                new [] {
                    IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID,
                    IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID+1,
                    IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID+2
                }
            );
        bool mixedTags = 
            await EFDataSourceLogic.CheckIfTagsExist_Logic(
                context,
                new [] {
                    IDSet.Tag1ID , IDSet.Tag2ID,
                    IDSet.Tag1ID+IDSet.Tag2ID+IDSet.Tag3ID+2
                }
            );
        
        Assert.True(existingTags);
        Assert.False(nonExistingTags);
        Assert.False(mixedTags);
    }

    [Fact]
    public async void CheckIfNodeExists_WillReturnTrueOnlyIfSpecifiedNodeExists() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool ExistingNodes =
            await EFDataSourceLogic.CheckIfNodeExists_Logic(
                context, IDSet.Node1ID
            ) &&
            await EFDataSourceLogic.CheckIfNodeExists_Logic(
                context, IDSet.Node2ID
            ) &&
            await EFDataSourceLogic.CheckIfNodeExists_Logic(
                context, IDSet.Node3ID
            );
        bool NonExistingNodes =
            await EFDataSourceLogic.CheckIfNodeExists_Logic(
                context, IDSet.Node1ID+IDSet.Node2ID+IDSet.Node3ID
            ) ||
            await EFDataSourceLogic.CheckIfNodeExists_Logic(
                context, IDSet.Node1ID+IDSet.Node2ID+IDSet.Node3ID+1
            ) ||
            await EFDataSourceLogic.CheckIfNodeExists_Logic(
                context, IDSet.Node1ID+IDSet.Node2ID+IDSet.Node3ID+2
            );
        
        Assert.True(ExistingNodes);
        Assert.False(NonExistingNodes);
    }

    [Fact]
    public async void CheckIfTagNameExists_WillReturnTrueOnlyIfTagWithSpecifiedNameIsAlreadyInTheDatabase() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool ExistingTagNames =
            await EFDataSourceLogic.CheckIfTagNameExists_Logic(
                context, utils.FirstTagName
            ) &&
            await EFDataSourceLogic.CheckIfTagNameExists_Logic(
                context, utils.SecondTagName
            ) &&
            await EFDataSourceLogic.CheckIfTagNameExists_Logic(
                context, utils.ThirdTagName
            );
        bool NonExistingTagNames =
            await EFDataSourceLogic.CheckIfTagNameExists_Logic(
                context, "TestTagNonexistantName1"
            ) ||
            await EFDataSourceLogic.CheckIfTagNameExists_Logic(
                context, "Shvabra Cadabra"
            ) ||
            await EFDataSourceLogic.CheckIfTagNameExists_Logic(
                context, "1234321"
            );
        
        Assert.True(ExistingTagNames);
        Assert.False(NonExistingTagNames);
    }

    [Fact]
    public async void CheckIfNodeNameExists_WillReturnTrueOnlyIfNodeWithSpecifiedNameIsAlreadyInTheDatabase() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool ExistingNodeNames =
            await EFDataSourceLogic.CheckIfNodeNameExists_Logic(
                context, utils.FirstNodeName
            ) &&
            await EFDataSourceLogic.CheckIfNodeNameExists_Logic(
                context, utils.SecondNodeName
            ) &&
            await EFDataSourceLogic.CheckIfNodeNameExists_Logic(
                context, utils.ThirdNodeName
            );
        bool NonExistingNodeNames =
            await EFDataSourceLogic.CheckIfNodeNameExists_Logic(
                context, "TestNodeNonexistantName1"
            ) ||
            await EFDataSourceLogic.CheckIfNodeNameExists_Logic(
                context, "Shvabra Cadabra"
            ) ||
            await EFDataSourceLogic.CheckIfNodeNameExists_Logic(
                context, "1234321"
            );
        
        Assert.True(ExistingNodeNames);
        Assert.False(NonExistingNodeNames);
    }

    [Fact]
    public async void CheckIfCWSNameExists_WillReturnTrueOnlyIfCWSWithSpecifiedNameIsAlreadyInTheDatabase() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool ExistingCWSName =
            await EFDataSourceLogic.CheckIfCWSNameExists_Logic(
                context, utils.WebInterfaceOn8080Name
            );
        bool NonExistingCWSNames =
            await EFDataSourceLogic.CheckIfCWSNameExists_Logic(
                context, "TestCWSNonexistantName1"
            ) ||
            await EFDataSourceLogic.CheckIfCWSNameExists_Logic(
                context, "Shvabra Cadabra"
            ) ||
            await EFDataSourceLogic.CheckIfCWSNameExists_Logic(
                context, "1234321"
            );
        
        Assert.True(ExistingCWSName);
        Assert.False(NonExistingCWSNames);
    }

    [Fact]
    public async void CheckIfProfileNameExists_WillReturnTrueOnlyIfProfileWithSpecifiedNameIsAlreadyInTheDatabase() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        bool ExistingProfileName =
            await EFDataSourceLogic.CheckIfProfileNameExists_Logic(
                context, context.Profiles.First().Name
            );
        bool NonExistingProfileNames =
            await EFDataSourceLogic.CheckIfProfileNameExists_Logic(
                context, "TestNodeNonexistantName1"
            ) ||
            await EFDataSourceLogic.CheckIfProfileNameExists_Logic(
                context, "Shvabra Cadabra"
            ) ||
            await EFDataSourceLogic.CheckIfProfileNameExists_Logic(
                context, "1234321"
            );
        
        Assert.True(ExistingProfileName);
        Assert.False(NonExistingProfileNames);
    }

    [Fact]
    public async void CreateTag_WillCreateNewTag() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        int tagsBefore = context.Tags.Count();
        NodeTag tag = new NodeTag {
            ID = 0,
            Name = "newTag"
        };
        NodeTag newTag = await EFDataSourceLogic.CreateTag_Logic(context, tag);
        NodeTag newTagWithParentNetwrokInclude = context.Tags
            .AsNoTracking()
            .Single(t => t.ID == newTag.ID);
        int tagsAfter = context.Tags.Count();

        Assert.Equal(1, tagsAfter - tagsBefore);
        Assert.Single(context.Tags, t => t.Name == "newTag");
    }

    [Fact]
    public static async Task CreateWebServiceBinding_WillBindWebServiceToANode() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var secondWebServiceID = utils.AddSecondWebService(context);
        var bindingsBefore = context.WebServiceBindings.AsNoTracking().ToArray();

        await EFDataSourceLogic.CreateWebServiceBinding_Logic(
            context, IDSet.Node1ID, secondWebServiceID, "80", null, null);
        await EFDataSourceLogic.CreateWebServiceBinding_Logic(
            context, IDSet.Node2ID, secondWebServiceID, "", null, null);
        await EFDataSourceLogic.CreateWebServiceBinding_Logic(
            context, IDSet.Node3ID, secondWebServiceID, "8080", null, null);
        var bindingsAfter = context.WebServiceBindings.AsNoTracking().ToArray();
        var bindingsForNode1 = bindingsAfter.Where(b => b.NodeID == IDSet.Node1ID);
        var bindingsForNode2 = bindingsAfter.Where(b => b.NodeID == IDSet.Node2ID);
        var bindingsForNode3 = bindingsAfter.Where(b => b.NodeID == IDSet.Node3ID);

        Assert.Equal(2, bindingsBefore.Count());
        Assert.Equal(5, bindingsAfter.Count());
        Assert.Equal(2, bindingsForNode1.Count());
        Assert.Equal(2, bindingsForNode2.Count());
        Assert.Single(bindingsForNode3);
        Assert.Single(bindingsForNode1, b => b.ServiceID == IDSet.WebServiceID);
        Assert.Single(bindingsForNode2, b => b.ServiceID == IDSet.WebServiceID);
        Assert.Single(bindingsForNode1, b => b.ServiceID == secondWebServiceID);
        Assert.Single(bindingsForNode2, b => b.ServiceID == secondWebServiceID);
        Assert.Single(bindingsForNode3, b => b.ServiceID == secondWebServiceID);
    }

    [Fact]
    public async void SetNodeTags_AttachesListOfTagsToTheNodeOnceEachAndRemovesNotSpecifiedTagsAlreadyAttached() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        var attachedBefore = context.TagAttachments
            .AsNoTracking()
            .Where(a => a.NodeID == IDSet.Node1ID)
            .ToList();
        await EFDataSourceLogic.SetNodeTags_Logic(context, IDSet.Node1ID, new [] {
            IDSet.Tag1ID, IDSet.Tag2ID
        });
        var attachedAfter = context.TagAttachments
            .AsNoTracking()
            .Where(a => a.NodeID == IDSet.Node1ID)
            .ToList();
        await EFDataSourceLogic.SetNodeTags_Logic(context, IDSet.Node1ID, new [] {
            IDSet.Tag3ID
        });
        var attachedAfterSecondChange = context.TagAttachments
            .AsNoTracking()
            .Where(a => a.NodeID == IDSet.Node1ID)
            .ToList();

        Assert.Single(attachedBefore);
        Assert.Equal(2, attachedAfter.Count);
        Assert.Single(attachedAfterSecondChange);
        Assert.Single(attachedAfter, a => a.TagID == IDSet.Tag1ID);
        Assert.Single(attachedAfter, a => a.TagID == IDSet.Tag2ID);
        Assert.Single(attachedAfterSecondChange, a => a.TagID == IDSet.Tag3ID);
    }
    
    [Fact]
    public async void SetProfileViewTagsSelection_WillSetListOfTagsUsedToFilterProfileView() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.SetProfileViewTagsTo2And3AndMonitorTagsTo1(context, IDSet);
        var selection = context.ProfilesTagSelection
            .AsNoTracking()
            .Where(pvt => pvt.BindedProfileID == IDSet.ProfileID &&
                        ProfileSelectedTagFlags.NodesListView ==
                            (pvt.Flags & ProfileSelectedTagFlags.NodesListView));
        var setBefore = selection.ToList();
        await EFDataSourceLogic.SetProfileViewTagsSelection_Logic(context, IDSet.ProfileID, new [] {
            IDSet.Tag1ID, IDSet.Tag2ID, IDSet.Tag3ID
        });
        var setAfter = selection.ToList();
        await EFDataSourceLogic.SetProfileViewTagsSelection_Logic(context, IDSet.ProfileID, new [] {
            IDSet.Tag2ID
        });
        var setAfterSecondChange = selection.ToList();

        Assert.Equal(2, setBefore.Count);
        Assert.Equal(3, setAfter.Count);
        Assert.Single(setAfterSecondChange);
        Assert.Single(setAfter, a => a.TagID == IDSet.Tag1ID);
        Assert.Single(setAfter, a => a.TagID == IDSet.Tag2ID);
        Assert.Single(setAfter, a => a.TagID == IDSet.Tag3ID);
        Assert.Single(setAfterSecondChange, a => a.TagID == IDSet.Tag2ID);
    }
    
    [Fact]
    public async void SetProfileMonitorTagsSelection_WillSetListOfTagsUsedToFilterProfileMonitor() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.SetProfileViewTagsTo2And3AndMonitorTagsTo1(context, IDSet);
        var selection = context.ProfilesTagSelection
            .AsNoTracking()
            .Where(pvt => pvt.BindedProfileID == IDSet.ProfileID &&
                        ProfileSelectedTagFlags.Monitor ==
                            (pvt.Flags & ProfileSelectedTagFlags.Monitor));
        var setBefore = selection.ToList();
        await EFDataSourceLogic.SetProfileMonitorTagsSelection_Logic(context, IDSet.ProfileID, new [] {
            IDSet.Tag1ID, IDSet.Tag2ID, IDSet.Tag3ID
        });
        var setAfter = selection.ToList();
        await EFDataSourceLogic.SetProfileMonitorTagsSelection_Logic(context, IDSet.ProfileID, new [] {
            IDSet.Tag2ID
        });
        var setAfterSecondChange = selection.ToList();

        Assert.Single(setBefore);
        Assert.Equal(3, setAfter.Count);
        Assert.Single(setAfterSecondChange);
        Assert.Single(setAfter, a => a.TagID == IDSet.Tag1ID);
        Assert.Single(setAfter, a => a.TagID == IDSet.Tag2ID);
        Assert.Single(setAfter, a => a.TagID == IDSet.Tag3ID);
        Assert.Single(setAfterSecondChange, a => a.TagID == IDSet.Tag2ID);
    }
    
    [Fact]
    public async void SetProfileViewTagsSelectionToProfileMonitorTagsSelection_WillSetAllViewTagSelectionsToCurrentMonitorTagsSelections() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
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
        utils.SetProfileViewTagsTo2And3AndMonitorTagsTo1(context, IDSet);
        var set1MonitorBefore = monitorSelection.ToList();
        var set1ViewBefore = viewSelection.ToList();
        await EFDataSourceLogic
            .SetProfileViewTagsSelectionToProfileMonitorTagsSelection_Logic(
                context, IDSet.ProfileID
            );
        var set1MonitorAfter = monitorSelection.ToList();
        var set1ViewAfter = viewSelection.ToList();
        //Test on the second set
        utils.SetProfileViewTagsTo1And2AndMonitorTagsTo2And3(context, IDSet);
        var set2MonitorBefore = monitorSelection.ToList();
        var set2ViewBefore = viewSelection.ToList();
        await EFDataSourceLogic
            .SetProfileViewTagsSelectionToProfileMonitorTagsSelection_Logic(
                context, IDSet.ProfileID
            );
        var set2MonitorAfter = monitorSelection.ToList();
        var set2ViewAfter = viewSelection.ToList();
        
        //Set1Assert
        Assert.Equal(2, set1ViewBefore.Count);
        Assert.Single(set1MonitorBefore, pst => pst.TagID == IDSet.Tag1ID);
        Assert.Single(set1ViewAfter, pst => pst.TagID == IDSet.Tag1ID);
        Assert.Single(set1MonitorAfter, pst => pst.TagID == IDSet.Tag1ID);
        //Set2Assert
        Assert.Equal(2, set2ViewBefore.Count);
        Assert.Equal(2, set2MonitorBefore.Count);
        Assert.Single(set2ViewBefore, pst => pst.TagID == IDSet.Tag1ID);
        Assert.Single(set2ViewBefore, pst => pst.TagID == IDSet.Tag2ID);
        Assert.Single(set2MonitorBefore, pst => pst.TagID == IDSet.Tag2ID);
        Assert.Single(set2MonitorBefore, pst => pst.TagID == IDSet.Tag3ID);
        Assert.Equal(2, set2ViewAfter.Count);
        Assert.Equal(2, set2MonitorAfter.Count);
        Assert.Single(set2ViewAfter, pst => pst.TagID == IDSet.Tag2ID);
        Assert.Single(set2ViewAfter, pst => pst.TagID == IDSet.Tag3ID);
        Assert.Single(set2MonitorAfter, pst => pst.TagID == IDSet.Tag2ID);
        Assert.Single(set2MonitorAfter, pst => pst.TagID == IDSet.Tag3ID);
    }

    [Fact]
    public async void CreateCustomWebService_CreatesNewWebService() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        CustomWebService cvs = new CustomWebService {
            ID = 0,
            ServiceName = "Web Interface On Custom Port",
            ServiceStr = "http://{node_ip}:{param1}",
            Parametr1Name = "Service Port"
        };
        int cwsCoundBefore = context.WebServices.Count();
        var createdCWS = await EFDataSourceLogic.CreateCustomWebService_Logic(context, cvs);
        int cwsCoundAfter = context.WebServices.Count();

        Assert.Equal(1, cwsCoundAfter - cwsCoundBefore);
        Assert.Equal("Web Interface On Custom Port", createdCWS.ServiceName);
    }

    [Fact]
    public async void UpdateProfile_WillUpdateSpecifiedProfile() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
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

        await EFDataSourceLogic.UpdateProfile_Logic(context, profileToUpdate);
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var nodeBefore = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.Node1ID);
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

        await EFDataSourceLogic.UpdateNode_Logic(context, nodeToUpdate);
        var nodeAfter = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.Node1ID);

        Assert.Equal(newName, nodeAfter.Name);
        Assert.Equal(newTestIP, nodeAfter.ip);
        Assert.NotEqual(nodeBefore.OpenPing, nodeAfter.OpenPing);
        Assert.NotEqual(nodeBefore.OpenSSH, nodeAfter.OpenSSH);
        Assert.NotEqual(nodeBefore.OpenTelnet, nodeAfter.OpenTelnet);
    }

    [Fact]
    public async void UpdateNode_WillNotChangeNodeParentAndDepth() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var nodeBefore = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.Node1ID);
        var nodeToUpdate = new NtwkNode {
            ID = nodeBefore.ID,
            ParentID = IDSet.Node3ID,
            Name = nodeBefore.Name,
            ip = nodeBefore.ip,
            OpenPing = nodeBefore.OpenPing,
            OpenSSH = nodeBefore.OpenSSH,
            OpenTelnet = nodeBefore.OpenTelnet
        };

        await EFDataSourceLogic.UpdateNode_Logic(context, nodeToUpdate);
        var nodeAfter = context.Nodes
            .AsNoTracking()
            .Single(n => n.ID == IDSet.Node1ID);

        Assert.NotEqual(IDSet.Node3ID, nodeBefore.ParentID);
        Assert.Equal(nodeBefore.ParentID, nodeAfter.ParentID);
    }

    [Fact]
    public async void CheckIfNodeInSubtree_WillReturnTrueOnlyIfSpecifiedNodeIsInSubtreeWitSpecifiedRoot() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.CreateClosuresForTestNodes(context, IDSet);

        //Checks For Node 1
        bool isNode1InNode1Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node1ID, IDSet.Node1ID);
        bool isNode1InNode2Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node1ID, IDSet.Node2ID);
        bool isNode1InNode3Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node1ID, IDSet.Node3ID);
        //Checks for node  2
        bool isNode2InNode1Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node2ID, IDSet.Node1ID);
        bool isNode2InNode2Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node2ID, IDSet.Node2ID);
        bool isNode2InNode3Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node2ID, IDSet.Node3ID);
        //Checks for node  3
        bool isNode3InNode1Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node3ID, IDSet.Node1ID);
        bool isNode3InNode2Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node3ID, IDSet.Node2ID);
        bool isNode3InNode3Subtree =
            await EFDataSourceLogic.CheckIfNodeInSubtree_Logic(
                context, IDSet.Node3ID, IDSet.Node3ID);
        
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
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.CreateClosuresForTestNodes(context, IDSet);
        var closuresBefore = context.NodesClosureTable.AsNoTracking().ToList();
        int? nodeParentIDBefore = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == IDSet.Node1ID)
            .Parent?.ID;
        testOutput.WriteLine("Closures before.");
        testOutput.WriteLine("[\n{0}]", string.Join(",\n",
            context.NodesClosureTable.AsNoTracking()
                .Select(c => (new {c.ID, c.AncestorID, c.DescendantID, c.Distance}).ToString())
                .ToArray()
            )
        );
        
        await EFDataSourceLogic.MoveNodesSubtree_Logic(context, IDSet.Node1ID, IDSet.Node3ID);
        var closuresAfter = context.NodesClosureTable.AsNoTracking().ToList();
        int? nodeParentIDAfter = context.Nodes
            .AsNoTracking()
            .Include(n => n.Parent)
            .Single(n => n.ID == IDSet.Node1ID)
            .Parent?.ID;
        testOutput.WriteLine("Closures after.");
        testOutput.WriteLine("[\n{0}]", string.Join(",\n",
            context.NodesClosureTable.AsNoTracking()
                .Select(c => (new {c.ID, c.AncestorID, c.DescendantID, c.Distance}).ToString())
                .ToArray()
            )
        );

        Assert.Null(nodeParentIDBefore);
        Assert.Equal(IDSet.Node3ID, nodeParentIDAfter);
        Assert.Equal(2, closuresBefore.Where(c => c.DescendantID == IDSet.Node1ID).Count());
        Assert.Equal(0, closuresBefore.Where(c => c.DescendantID == IDSet.Node1ID).Sum(c => c.Distance));
        Assert.Equal(3, closuresAfter.Where(c => c.DescendantID == IDSet.Node1ID).Count());
        Assert.Equal(2, closuresAfter.Where(c => c.DescendantID == IDSet.Node1ID).Sum(c => c.Distance));
        Assert.Equal(3, closuresBefore.Where(c => c.DescendantID == IDSet.Node2ID).Count());
        Assert.Equal(2, closuresBefore.Where(c => c.DescendantID == IDSet.Node2ID).Sum(c => c.Distance));
        Assert.Equal(4, closuresAfter.Where(c => c.DescendantID == IDSet.Node2ID).Count());
        Assert.Equal(5, closuresAfter.Where(c => c.DescendantID == IDSet.Node2ID).Sum(c => c.Distance));
    }

    [Fact]
    public async void UpdateCustomWebService_WillUpdateSpecifiedWebService() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var cwsBefore = context.WebServices.AsNoTracking()
            .Single(ws => ws.ID == IDSet.WebServiceID);
        string newCWSName = "TestService";
        string newCWSStr = "http://test.{param1}.com";
        string newCWSParam1Name = "TestParam1";
        var wsToUpdate = new CustomWebService {
            ID=cwsBefore.ID,
            ServiceName=newCWSName,
            ServiceStr=newCWSStr,
            Parametr1Name=newCWSParam1Name
        };

        await EFDataSourceLogic.UpdateCustomWebService_Logic(context, wsToUpdate);
        var cwsAfter = context.WebServices.AsNoTracking()
            .Single(ws => ws.ID == IDSet.WebServiceID);

        Assert.NotEqual(newCWSName, cwsBefore.ServiceName);
        Assert.NotEqual(newCWSStr, cwsBefore.ServiceStr);
        Assert.NotEqual(newCWSParam1Name, cwsBefore.Parametr1Name);
        Assert.Equal(newCWSName, cwsAfter.ServiceName);
        Assert.Equal(newCWSStr, cwsAfter.ServiceStr);
        Assert.Equal(newCWSParam1Name, cwsAfter.Parametr1Name);
    }

    [Fact]
    public async Task UpdateWebServiceBinding_WillUpdateBindingParametersOfSpecifiedBinding() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var secondWebServiceID = utils.AddSecondWebService(context);
        utils.CreateBindingsForSecondWebService(
            context, IDSet, secondWebServiceID);

        await EFDataSourceLogic.UpdateWebServiceBinding_Logic(
            context, IDSet.Node1ID, secondWebServiceID, "123", null, null
        );
        await EFDataSourceLogic.UpdateWebServiceBinding_Logic(
            context, IDSet.Node2ID, secondWebServiceID, "421", null, null
        );
        await EFDataSourceLogic.UpdateWebServiceBinding_Logic(
            context, IDSet.Node3ID, secondWebServiceID, "test", null, null
        );

        Assert.Equal("123", context.WebServiceBindings
            .Single(b => b.ServiceID == secondWebServiceID && b.NodeID == IDSet.Node1ID)
            .Param1);
        Assert.Equal("421", context.WebServiceBindings
            .Single(b => b.ServiceID == secondWebServiceID && b.NodeID == IDSet.Node2ID)
            .Param1);
        Assert.Equal("test", context.WebServiceBindings
            .Single(b => b.ServiceID == secondWebServiceID && b.NodeID == IDSet.Node3ID)
            .Param1);
    }

    [Fact]
    public async Task RemoveProfile_WillRemoveSpecifiedProfileItsTagBindingsAndMonitoringSessions() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.SetProfileViewTagsTo2And3AndMonitorTagsTo1(context, IDSet);
        
        await EFDataSourceLogic.RemoveProfile_Logic(context, IDSet.ProfileID);

        Assert.Empty(context.Profiles);
        Assert.Empty(context.ProfilesTagSelection);
        Assert.Empty(context.MonitoringSessions.Where(s => s.CreatedByProfileID == IDSet.ProfileID));
    }

    [Fact]
    public async Task RemoveNode_WillRemoveSpecifiedNodeAndThrowsInvalidOperationIfNodeContainsChildren() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);

        var nodesBefore = context.Nodes.AsNoTracking().ToArray();
        await EFDataSourceLogic.RemoveNode_Logic(context, IDSet.Node3ID);
        var nodesAfter = context.Nodes.AsNoTracking().ToArray();

        Assert.Equal(3, nodesBefore.Length);
        Assert.Single(nodesBefore, n => n.ID == IDSet.Node3ID);
        Assert.Equal(2, nodesAfter.Length);
        Assert.DoesNotContain(nodesAfter, n => n.ID == IDSet.Node3ID);
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await EFDataSourceLogic.RemoveNode_Logic(context, IDSet.Node1ID)
        );
    }

    [Fact]
    public async Task RemoveTag_WillRemoveTagAndAllTheLinkingEntitiesThatUseIt() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        utils.SetProfileViewTagsTo1And2AndMonitorTagsTo2And3(context, IDSet);

        var tagsBefore = context.Tags.AsNoTracking().ToArray();
        bool wasAttachedToNodes = context.TagAttachments.Any(a => a.TagID == IDSet.Tag1ID);
        bool hadProfileViewBindings = context.ProfilesTagSelection.Any(a => a.TagID == IDSet.Tag1ID);
        await EFDataSourceLogic.RemoveTag_Logic(context, IDSet.Tag1ID);
        var tagsAfter = context.Tags.AsNoTracking().ToArray();
        bool isAttachedToNodes = context.TagAttachments.Any(a => a.TagID == IDSet.Tag1ID);
        bool hasProfileViewBindings = context.ProfilesTagSelection.Any(a => a.TagID == IDSet.Tag1ID);

        Assert.Single(tagsBefore, t => t.ID == IDSet.Tag1ID);
        Assert.True(wasAttachedToNodes);
        Assert.True(hadProfileViewBindings);
        Assert.DoesNotContain(tagsAfter, t => t.ID == IDSet.Tag1ID);
        Assert.False(isAttachedToNodes);
        Assert.False(hasProfileViewBindings);
    }

    [Fact]
    public async Task RemoveCustomWebService_WillRemoveSpecifiedWebServiceAndAllItsBindingsToNodes() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var secondWebServiceID = utils.AddSecondWebService(context);
        utils.CreateBindingsForSecondWebService(
            context, IDSet, secondWebServiceID);
        bool existedBefore = null != context.WebServices
            .SingleOrDefault(ws => ws.ID == secondWebServiceID);
        bool hadBindings = context.WebServiceBindings
            .Any(b => b.ServiceID == secondWebServiceID);

        await EFDataSourceLogic.RemoveCustomWebService_Logic(context, secondWebServiceID);

        Assert.True(existedBefore);
        Assert.True(hadBindings);
        Assert.Null(context.WebServices
            .SingleOrDefault(ws => ws.ID == secondWebServiceID));
        Assert.Empty(context.WebServiceBindings
            .Where(b => b.ServiceID == secondWebServiceID));
    }
    
    [Fact]
    public async Task RemoveWebServiceBinding_WillRemoveSpecifiedBinding() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var context = utils.GetEmptyContext();
        var IDSet = utils.AddTestDataSet(context);
        var secondWebServiceID = utils.AddSecondWebService(context);
        utils.CreateBindingsForSecondWebService(
            context, IDSet, secondWebServiceID);
        int bindingsBefore = context.WebServiceBindings.Count();

        await EFDataSourceLogic.RemoveWebServiceBinding_Logic(context,
            IDSet.Node1ID, IDSet.WebServiceID);
        await EFDataSourceLogic.RemoveWebServiceBinding_Logic(context,
            IDSet.Node2ID, secondWebServiceID);
        await EFDataSourceLogic.RemoveWebServiceBinding_Logic(context,
            IDSet.Node3ID, secondWebServiceID);
        int bindingsAfter = context.WebServiceBindings.Count();

        Assert.Equal(3, bindingsBefore - bindingsAfter);
        Assert.Single(context.WebServiceBindings.Where(b => b.NodeID == IDSet.Node1ID));
        Assert.Single(context.WebServiceBindings.Where(b => b.NodeID == IDSet.Node2ID));
        Assert.Empty(context.WebServiceBindings.Where(b => b.NodeID == IDSet.Node3ID));
    }
}

}
