using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.EFDatabase;

namespace Tests {

class EFDatabaseMockingUtils {
    public readonly string TestProfileName = "TestProfile";

    public readonly string FirstNodeName = "TestNode1";
    public readonly string SecondNodeName = "TestNode2";
    public readonly string ThirdNodeName = "TestNode3";
    
    public readonly string FirstTagName = "TestTag1";
    public readonly string SecondTagName = "TestTag2";
    public readonly string ThirdTagName = "TestTag3";

    public readonly string WebInterfaceOn8080Name = "Web Interface on 8080";

    public struct TestIDsSet {
        public int ProfileID;
        public int Node1ID;
        public int Node2ID;
        public int Node3ID;
        public int Tag1ID;
        public int Tag2ID;
        public int Tag3ID;
        public int WebServiceID;
    }
    
    public NtwkDBContext GetEmptyContext() {
        var options = new DbContextOptionsBuilder<NtwkDBContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
        var context = new NtwkDBContext(options);
        return context;
    }

    public TestIDsSet AddTestDataSet(NtwkDBContext context) {

        var testProfile = new Profile {
            ID = 0,
            Name = TestProfileName,
            StartMonitoringOnLaunch = true,
            DepthMonitoring = true,
            MonitoringStartHour = 0,
            MonitoringSessionDuration = 24,
            MonitorInterval = 10
        };
        context.Profiles.Add(testProfile);
        context.SaveChanges();

        
        var testTag1 = new NodeTag {
            ID = 0,
            Name = FirstTagName
        };
        var testTag2 = new NodeTag {
            ID = 0,
            Name = SecondTagName
        };
        var testTag3 = new NodeTag {
            ID = 0,
            Name = ThirdTagName
        };
        context.Tags.Add(testTag1);
        context.Tags.Add(testTag2);
        context.Tags.Add(testTag3);
        context.SaveChanges();

        var testNode1 = new NtwkNode {
            ID = 0,
            Parent = null,
            Name = FirstNodeName,
            //NodeDepth = 0,
            ip = 167837697,
            OpenTelnet = false,
            OpenSSH = true,
            OpenPing = true
        };
        context.Nodes.Add(testNode1);
        context.SaveChanges();
        var testNode2 = new NtwkNode {
            ID = 0,
            Parent = testNode1,
            Name = SecondNodeName,
            //NodeDepth = 1,
            ip = 3232292602,
            OpenTelnet = true,
            OpenSSH = false,
            OpenPing = true
        };
        var testNode3 = new NtwkNode {
            ID = 0,
            Parent = null,
            Name = ThirdNodeName,
            //NodeDepth = 0,
            ip = 167837697,
            OpenTelnet = false,
            OpenSSH = true,
            OpenPing = true
        };
        context.Nodes.Add(testNode2);
        context.Nodes.Add(testNode3);
        context.SaveChanges();

        var webInterfaceService = new CustomWebService {
            ID = 0,
            ServiceName = WebInterfaceOn8080Name,
            ServiceStr = "http://{node_ip}:8080"
        };
        context.WebServices.Add(webInterfaceService);
        context.SaveChanges();

        var tagAttacmentNode1_Tag1 = new TagAttachment {
            ID = 0,
            Tag = testTag1,
            Node = testNode1,
        };
        var tagAttacmentNode2_Tag1 = new TagAttachment {
            ID = 0,
            Tag = testTag1,
            Node = testNode2,
        };
        var tagAttacmentNode2_Tag2 = new TagAttachment {
            ID = 0,
            Tag = testTag2,
            Node = testNode2,
        };
        var tagAttacmentNode3_Tag3 = new TagAttachment {
            ID = 0,
            Tag = testTag3,
            Node = testNode3,
        };
        context.TagAttachments.Add(tagAttacmentNode1_Tag1);
        context.TagAttachments.Add(tagAttacmentNode2_Tag1);
        context.TagAttachments.Add(tagAttacmentNode2_Tag2);
        context.TagAttachments.Add(tagAttacmentNode3_Tag3);
        context.SaveChanges();

        var webServiceBinding_Node1 = new CustomWSBinding {
            ID = 0,
            Service = webInterfaceService,
            Node = testNode1
        };
        var webServiceBinding_Node2 = new CustomWSBinding {
            ID = 0,
            Service = webInterfaceService,
            Node = testNode2
        };
        context.WebServiceBindings.Add(webServiceBinding_Node1);
        context.WebServiceBindings.Add(webServiceBinding_Node2);
        context.SaveChanges();

        
        var testSession1 = new MonitoringSession {
            ID = 0,
            CreatedByProfile = testProfile,
            ParticipatingNodesNum = 2,
            CreationTime = 1528359015,
            LastPulseTime = 1528360285,
        };
        var monPulse1_S1 = new MonitoringPulseResult {
            ID = 0,
            Responded = 2,
            Silent = 0,
            Skipped = 0,
            CreationTime = 1528359015
        };
        var monPulse2_S1 = new MonitoringPulseResult {
            ID = 0,
            Responded = 0,
            Silent = 1,
            Skipped = 1,
            CreationTime = 1528360285
        };
        var monMessage_S1P2 = new MonitoringMessage {
            ID = 0,
            MessageType = MonitoringMessageType.Danger_NoPingReturned_SkippedChildren,
            MessageSourceNodeName = FirstNodeName,
            NumSkippedChildren = 0
        };

        context.MonitoringSessions.Add(testSession1);
        context.SaveChanges();
        context.MonitoringPulses.Add(monPulse1_S1);
        context.SaveChanges();
        context.MonitoringPulses.Add(monPulse2_S1);
        context.SaveChanges();
        context.MonitoringMessages.Add(monMessage_S1P2);
        context.SaveChanges();
        monPulse2_S1 = context.MonitoringPulses
            .Include(p => p.Messages)
            .Single(p => p.ID == monPulse2_S1.ID);
        monPulse2_S1.Messages.Add(monMessage_S1P2);
        context.SaveChanges();
        testSession1 = context.MonitoringSessions
            .Include(s => s.Pulses)
            .Single(s => s.ID == testSession1.ID);
        testSession1.Pulses.Add(monPulse1_S1);
        testSession1.Pulses.Add(monPulse2_S1);
        context.SaveChanges();

        return new TestIDsSet {
            ProfileID = context.Profiles.Single(n => n.Name == TestProfileName).ID,
            Node1ID = context.Nodes.Single(n => n.Name == FirstNodeName).ID,
            Node2ID = context.Nodes.Single(n => n.Name == SecondNodeName).ID,
            Node3ID = context.Nodes.Single(n => n.Name == ThirdNodeName).ID,
            Tag1ID = context.Tags.Single(t => t.Name == FirstTagName).ID,
            Tag2ID = context.Tags.Single(t => t.Name == SecondTagName).ID,
            Tag3ID = context.Tags.Single(t => t.Name == ThirdTagName).ID,
            WebServiceID = context.WebServices.Single(w => w.ServiceName == WebInterfaceOn8080Name).ID
        };
    }

    public void CreateClosuresForTestNodes(NtwkDBContext context, TestIDsSet IDSet) {
        context.AddRange(new [] {//Node1
            new NodeClosure {
                ID=0,
                AncestorID = null,
                DescendantID = IDSet.Node1ID,
                Distance = 0
            },
            new NodeClosure {
                ID=0,
                AncestorID = IDSet.Node1ID,
                DescendantID = IDSet.Node1ID,
                Distance = 0
            },
        });
        context.AddRange(new [] {//Node2
            new NodeClosure {
                ID=0,
                AncestorID = null,
                DescendantID = IDSet.Node2ID,
                Distance = 1
            },
            new NodeClosure {
                ID=0,
                AncestorID = IDSet.Node1ID,
                DescendantID = IDSet.Node2ID,
                Distance = 1
            },
            new NodeClosure {
                ID=0,
                AncestorID = IDSet.Node2ID,
                DescendantID = IDSet.Node2ID,
                Distance = 0
            },
        });
        context.AddRange(new [] {//Node3
            new NodeClosure {
                ID=0,
                AncestorID = null,
                DescendantID = IDSet.Node3ID,
                Distance = 0
            },
            new NodeClosure {
                ID=0,
                AncestorID = IDSet.Node3ID,
                DescendantID = IDSet.Node3ID,
                Distance = 0
            },
        });
        context.SaveChanges();
    }

    public void SetProfileViewTagsTo1And2AndMonitorTagsTo2And3(NtwkDBContext context, TestIDsSet IDSet) {
        context.ProfilesTagSelection.RemoveRange(context.ProfilesTagSelection);
        var pvt = new [] {
            new ProfileSelectedTag {
                ID=0,
                Flags = ProfileSelectedTagFlags.NodesListView,
                TagID = IDSet.Tag1ID,
                BindedProfileID = IDSet.ProfileID
            },
            new ProfileSelectedTag {
                ID=0,
                Flags = ProfileSelectedTagFlags.NodesListView |
                    ProfileSelectedTagFlags.Monitor,
                TagID = IDSet.Tag2ID,
                BindedProfileID = IDSet.ProfileID
            },
            new ProfileSelectedTag {
                ID=0,
                Flags = ProfileSelectedTagFlags.Monitor,
                TagID = IDSet.Tag3ID,
                BindedProfileID = IDSet.ProfileID
            }
        };
        context.AddRange(pvt);
        context.SaveChanges();
    }

    public void SetProfileViewTagsTo2And3AndMonitorTagsTo1(NtwkDBContext context, TestIDsSet IDSet) {
        context.ProfilesTagSelection.RemoveRange(context.ProfilesTagSelection);
        var pvt = new [] {
            new ProfileSelectedTag {
                ID=0,
                Flags = ProfileSelectedTagFlags.Monitor,
                TagID = IDSet.Tag1ID,
                BindedProfileID = IDSet.ProfileID
            },
            new ProfileSelectedTag {
                ID=0,
                Flags = ProfileSelectedTagFlags.NodesListView,
                TagID = IDSet.Tag2ID,
                BindedProfileID = IDSet.ProfileID
            },
            new ProfileSelectedTag {
                ID=0,
                Flags = ProfileSelectedTagFlags.NodesListView,
                TagID = IDSet.Tag3ID,
                BindedProfileID = IDSet.ProfileID
            }
        };
        context.AddRange(pvt);
        context.SaveChanges();
    }

    public int AddSecondWebService(NtwkDBContext context) {
        string newServiceName = "NewWebService";
        string newServiceStr = "https://{node_ip}:{param1}";
        string newParam1Name = "Port";
        var secondWebService = new CustomWebService {
            ID=0,
            ServiceName=newServiceName,
            ServiceStr=newServiceStr,
            Parametr1Name=newParam1Name
        };
        context.Add(secondWebService);
        context.SaveChanges();
        return secondWebService.ID;
    }

    public void CreateBindingsForSecondWebService(
        NtwkDBContext context,
        TestIDsSet IDSet,
        int secondWebServiceID
    ) {
        var secondWSBindings = new [] {
            new CustomWSBinding {
                ID = 0,
                ServiceID = secondWebServiceID,
                NodeID = IDSet.Node1ID,
                Param1 = "80"
            },
            new CustomWSBinding {
                ID = 0,
                ServiceID = secondWebServiceID,
                NodeID = IDSet.Node2ID,
                Param1 = "55315"
            },
            new CustomWSBinding {
                ID = 0,
                ServiceID = secondWebServiceID,
                NodeID = IDSet.Node3ID,
                Param1 = ""
            },
        };
        context.WebServiceBindings.AddRange(secondWSBindings);
        context.SaveChanges();
    }
}

}