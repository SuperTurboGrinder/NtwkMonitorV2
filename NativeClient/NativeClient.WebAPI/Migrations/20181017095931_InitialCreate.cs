using Microsoft.EntityFrameworkCore.Migrations;

namespace NativeClient.WebAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentID = table.Column<int>(nullable: true),
                    ParentPort = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    ip = table.Column<uint>(nullable: false),
                    OpenTelnet = table.Column<bool>(nullable: false),
                    OpenSSH = table.Column<bool>(nullable: false),
                    OpenPing = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Nodes_Nodes_ParentID",
                        column: x => x.ParentID,
                        principalTable: "Nodes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    MonitoringStartHour = table.Column<int>(nullable: false),
                    MonitoringSessionDuration = table.Column<int>(nullable: false),
                    StartMonitoringOnLaunch = table.Column<bool>(nullable: false),
                    DepthMonitoring = table.Column<bool>(nullable: false),
                    MonitorInterval = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WebServices",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    ServiceStr = table.Column<string>(nullable: false),
                    Parametr1Name = table.Column<string>(nullable: true),
                    Parametr2Name = table.Column<string>(nullable: true),
                    Parametr3Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebServices", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NodesClosureTable",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AncestorID = table.Column<int>(nullable: true),
                    DescendantID = table.Column<int>(nullable: false),
                    Distance = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodesClosureTable", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NodesClosureTable_Nodes_AncestorID",
                        column: x => x.AncestorID,
                        principalTable: "Nodes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodesClosureTable_Nodes_DescendantID",
                        column: x => x.DescendantID,
                        principalTable: "Nodes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringSessions",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedByProfileID = table.Column<int>(nullable: false),
                    ParticipatingNodesNum = table.Column<int>(nullable: false),
                    CreationTime = table.Column<double>(nullable: false),
                    LastPulseTime = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringSessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MonitoringSessions_Profiles_CreatedByProfileID",
                        column: x => x.CreatedByProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfilesTagSelection",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Flags = table.Column<int>(nullable: false),
                    BindedProfileID = table.Column<int>(nullable: false),
                    TagID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilesTagSelection", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProfilesTagSelection_Profiles_BindedProfileID",
                        column: x => x.BindedProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfilesTagSelection_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagAttachments",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagID = table.Column<int>(nullable: false),
                    NodeID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAttachments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TagAttachments_Nodes_NodeID",
                        column: x => x.NodeID,
                        principalTable: "Nodes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagAttachments_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebServiceBindings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceID = table.Column<int>(nullable: false),
                    NodeID = table.Column<int>(nullable: false),
                    Param1 = table.Column<string>(nullable: true),
                    Param2 = table.Column<string>(nullable: true),
                    Param3 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebServiceBindings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WebServiceBindings_Nodes_NodeID",
                        column: x => x.NodeID,
                        principalTable: "Nodes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebServiceBindings_WebServices_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "WebServices",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringPulses",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Responded = table.Column<int>(nullable: false),
                    Silent = table.Column<int>(nullable: false),
                    Skipped = table.Column<int>(nullable: false),
                    CreationTime = table.Column<double>(nullable: false),
                    MonitoringSessionID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringPulses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MonitoringPulses_MonitoringSessions_MonitoringSessionID",
                        column: x => x.MonitoringSessionID,
                        principalTable: "MonitoringSessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringMessages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageType = table.Column<int>(nullable: false),
                    MessageSourceNodeName = table.Column<string>(nullable: false),
                    NumSkippedChildren = table.Column<int>(nullable: false),
                    MonitoringPulseResultID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MonitoringMessages_MonitoringPulses_MonitoringPulseResultID",
                        column: x => x.MonitoringPulseResultID,
                        principalTable: "MonitoringPulses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringMessages_MonitoringPulseResultID",
                table: "MonitoringMessages",
                column: "MonitoringPulseResultID");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringPulses_MonitoringSessionID",
                table: "MonitoringPulses",
                column: "MonitoringSessionID");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringSessions_CreatedByProfileID",
                table: "MonitoringSessions",
                column: "CreatedByProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ParentID",
                table: "Nodes",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_NodesClosureTable_AncestorID",
                table: "NodesClosureTable",
                column: "AncestorID");

            migrationBuilder.CreateIndex(
                name: "IX_NodesClosureTable_DescendantID",
                table: "NodesClosureTable",
                column: "DescendantID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfilesTagSelection_BindedProfileID",
                table: "ProfilesTagSelection",
                column: "BindedProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfilesTagSelection_TagID",
                table: "ProfilesTagSelection",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_TagAttachments_NodeID",
                table: "TagAttachments",
                column: "NodeID");

            migrationBuilder.CreateIndex(
                name: "IX_TagAttachments_TagID",
                table: "TagAttachments",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_WebServiceBindings_NodeID",
                table: "WebServiceBindings",
                column: "NodeID");

            migrationBuilder.CreateIndex(
                name: "IX_WebServiceBindings_ServiceID",
                table: "WebServiceBindings",
                column: "ServiceID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringMessages");

            migrationBuilder.DropTable(
                name: "NodesClosureTable");

            migrationBuilder.DropTable(
                name: "ProfilesTagSelection");

            migrationBuilder.DropTable(
                name: "TagAttachments");

            migrationBuilder.DropTable(
                name: "WebServiceBindings");

            migrationBuilder.DropTable(
                name: "MonitoringPulses");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "WebServices");

            migrationBuilder.DropTable(
                name: "MonitoringSessions");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
