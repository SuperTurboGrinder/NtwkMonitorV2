using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.EFDatabase;
using Data.DataServices.DbOperations;

namespace Tests {

static class EFDatabaseMockingUtils {
    public static readonly string TestProfileName = "TestProfile";

    public static readonly string[] NodeNames = new[] {
        "TestNode1",
        "TestNode2",
        "TestNode3"
    };
    public static readonly string[] TagNames = new[] {
        "TestTag1",
        "TestTag2",
        "TestTag3"
    };

    public static readonly string[] WebServicesNames = new[] {
        "Web Interface on 8080",
        "SecondTestService"
    };

    public struct TestIDsSet {
        public int ProfileID;
        public int[] WebServicesIDs;
        public int[] NodesIDs;
        public int[] TagsIDs;
    }
    
    public static NtwkDBContext GetEmptyContext() {
        var options = new DbContextOptionsBuilder<NtwkDBContext>()
                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                      .Options;
        var context = new NtwkDBContext(options);
        return context;
    }

    public static (NtwkDBContext, TestIDsSet, EFDataSource) GetTestDataContext() {
        NtwkDBContext context = GetEmptyContext();
        TestIDsSet idSet = AddTestDataSet(context);
        EFDataSource dataSource = new EFDataSource(
            context,
            new FileErrorLogger()
        );
        return (context, idSet, dataSource);
    }

    private static TestIDsSet AddTestDataSet(NtwkDBContext context) {
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

        NodeTag[] testTags = Enumerable.Range(0, 3)
            .Select(i => new NodeTag {
                ID = 0,
                Name = TagNames[i]
            }).ToArray();
        context.Tags.AddRange(testTags);
        context.SaveChanges();

        NtwkNode[] testNodes = new NtwkNode[3];
        {
            uint[] ips = new[] {
                167837697u,
                3232292602u,
                167837697u
            };
            bool[] t = new[] { false, true, false };
            bool[] s = new[] { true, false, true };
            for(int i = 0; i < 3; i++) {
                testNodes[i] = new NtwkNode {
                    ID = 0,
                    Parent = null,
                    Name = NodeNames[i],
                    ip = ips[i],
                    OpenTelnet = t[i],
                    OpenSSH = s[i],
                    OpenPing = true
                };
            }
            context.Nodes.Add(testNodes[0]);
            context.SaveChanges();
            testNodes[1].Parent = testNodes[0];
            context.Nodes.Add(testNodes[1]);
            context.Nodes.Add(testNodes[2]);
            context.SaveChanges();
        }
        

        CustomWebService[] testWebServices;
        {
            string[] wsStrs = new[] {
                "http://{node_ip}:8080",
                "https://{node_ip}:{param1}"
            };
            string[] wsP1 = new[] { null, "Port" };
            testWebServices = Enumerable.Range(0, 2)
                .Select(i => new CustomWebService {
                    ID = 0,
                    Name = WebServicesNames[i],
                    ServiceStr = wsStrs[i],
                    Parametr1Name = wsP1[i]
                }).ToArray();
            context.WebServices.AddRange(testWebServices);
            context.SaveChanges();
        }
        

        { //TagAttachments
            NodeTag[] t = new[] {
                testTags[0], testTags[0], testTags[1], testTags[2]
            };
            NtwkNode[] n = new[] {
                testNodes[0], testNodes[1], testNodes[1], testNodes[2]
            };
            context.TagAttachments.AddRange(Enumerable.Range(0, 4)
                .Select(i => new TagAttachment {
                    ID = 0,
                    Tag = t[i],
                    Node = n[i],
                })
            );
            context.SaveChanges();
        }


        {// WebServiceBindings
            CustomWebService[] s = new [] {
                testWebServices[0], testWebServices[0],
                testWebServices[1], testWebServices[1], testWebServices[1]
            };
            NtwkNode[] n = new[] {
                testNodes[0], testNodes[1],
                testNodes[0], testNodes[1], testNodes[2]
            };
            string[] p = new[] {
                null, null,
                "80", "55315", ""
            };
            context.WebServiceBindings.AddRange(Enumerable.Range(0, 4)
                .Select(i => new CustomWSBinding {
                    ID = 0,
                    Service = s[i],
                    Node = n[i],
                    Param1 = p[i]
                })
            );
            context.SaveChanges();
        }

        
        { // test monitoring session data
            MonitoringSession testSession = new MonitoringSession {
                ID = 0,
                CreatedByProfile = testProfile,
                ParticipatingNodesNum = 2,
                CreationTime = 1528359015,
                LastPulseTime = 1528360285,
            };
            MonitoringPulseResult[] pulses = new[] {
                new MonitoringPulseResult {
                    ID = 0,
                    Responded = 2,
                    Silent = 0,
                    Skipped = 0,
                    CreationTime = 1528359015
                },
                new MonitoringPulseResult {
                    ID = 0,
                    Responded = 0,
                    Silent = 1,
                    Skipped = 1,
                    CreationTime = 1528360285
                }
            };
            var monMessageForSecondPulse = new MonitoringMessage {
                ID = 0,
                MessageType = MonitoringMessageType.Danger_NoPingReturned_SkippedChildren,
                MessageSourceNodeName = NodeNames[0],
                NumSkippedChildren = 0
            };
            context.MonitoringSessions.Add(testSession);
            context.MonitoringPulses.AddRange(pulses);
            context.SaveChanges();
            pulses[1] = context.MonitoringPulses
                .Include(p => p.Messages)
                .Single(p => p.ID == pulses[1].ID);
            testSession = context.MonitoringSessions
                .Include(s => s.Pulses)
                .Single(s => s.ID == testSession.ID);
            testSession.Pulses.Add(pulses[0]);
            testSession.Pulses.Add(pulses[1]);
            pulses[1].Messages.Add(monMessageForSecondPulse);
            context.SaveChanges();
        }

        TestIDsSet idsSet = new TestIDsSet {
            ProfileID = testProfile.ID,
            WebServicesIDs = testWebServices.Select(s => s.ID).ToArray(),
            NodesIDs = testNodes.Select(n => n.ID).ToArray(),
            TagsIDs = testTags.Select(t => t.ID).ToArray(),
        };
        CreateClosuresForTestNodes(context, idsSet);
        return idsSet;
    }

    private static void CreateClosuresForTestNodes(NtwkDBContext context, TestIDsSet IDSet) {
        int?[] ancestors = new int?[] {
            null, IDSet.NodesIDs[0], //Node1
            null, IDSet.NodesIDs[0], IDSet.NodesIDs[1], //Node2
            null, IDSet.NodesIDs[1], //Node3
        };
        int[] descIndex = new int[] {
            0, 0,  1, 1, 1,  2, 2
        };
        int[] distance = new int[] {
            0, 0,  1, 1, 0,  0, 0
        };
        context.AddRange(Enumerable.Range(0, 7)
            .Select(i => new NodeClosure{
                ID=0,
                AncestorID = ancestors[0],
                DescendantID = IDSet.NodesIDs[descIndex[i]],
                Distance = distance[i]
            })
        );
        context.SaveChanges();
    }

    private static void SetProfileSelectedTags(
        NtwkDBContext context,
        TestIDsSet IDSet,
        ProfileSelectedTagFlags[] flags
    ) {
        context.ProfilesTagSelection.RemoveRange(context.ProfilesTagSelection);
        context.ProfilesTagSelection.AddRange(Enumerable.Range(0, 3)
            .Select(i => new ProfileSelectedTag {
                ID=0,
                Flags = flags[i],
                TagID = IDSet.TagsIDs[i],
                BindedProfileID = IDSet.ProfileID
            })
        );
        context.SaveChanges();
    }

    public static void SetProfileVTagsTo1_2AndMTagsTo2_3(NtwkDBContext context, TestIDsSet IDSet) {
        SetProfileSelectedTags(
            context,
            IDSet,
            new[] {
                ProfileSelectedTagFlags.NodesListView,
                ProfileSelectedTagFlags.NodesListView |
                    ProfileSelectedTagFlags.Monitor,
                ProfileSelectedTagFlags.Monitor
            }
        );
    }

    public static void SetProfileVTagsTo2_3AndMTagsTo1(NtwkDBContext context, TestIDsSet IDSet) {
        SetProfileSelectedTags(
            context,
            IDSet,
            new[] {
                ProfileSelectedTagFlags.Monitor,
                ProfileSelectedTagFlags.NodesListView,
                ProfileSelectedTagFlags.NodesListView
            }
        );
    }
}

}