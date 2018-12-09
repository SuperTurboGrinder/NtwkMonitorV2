﻿// <auto-generated />
using System;
using Data.EFDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NativeClient.WebAPI.Migrations
{
    [DbContext(typeof(NtwkDBContext))]
    partial class NtwkDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799");

            modelBuilder.Entity("Data.Model.EFDbModel.CustomWebService", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Parametr1Name");

                    b.Property<string>("Parametr2Name");

                    b.Property<string>("Parametr3Name");

                    b.Property<string>("ServiceStr")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("WebServices");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.CustomWSBinding", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("NodeID");

                    b.Property<string>("Param1");

                    b.Property<string>("Param2");

                    b.Property<string>("Param3");

                    b.Property<int>("ServiceID");

                    b.HasKey("ID");

                    b.HasIndex("NodeID");

                    b.HasIndex("ServiceID");

                    b.ToTable("WebServiceBindings");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.MonitoringMessage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("MessageSourceNodeName")
                        .IsRequired();

                    b.Property<int>("MessageType");

                    b.Property<int?>("MonitoringPulseResultID");

                    b.Property<int>("NumSkippedChildren");

                    b.HasKey("ID");

                    b.HasIndex("MonitoringPulseResultID");

                    b.ToTable("MonitoringMessages");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.MonitoringPulseResult", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("CreationTime");

                    b.Property<int?>("MonitoringSessionID");

                    b.Property<int>("Responded");

                    b.Property<int>("Silent");

                    b.Property<int>("Skipped");

                    b.HasKey("ID");

                    b.HasIndex("MonitoringSessionID");

                    b.ToTable("MonitoringPulses");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.MonitoringSession", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CreatedByProfileID");

                    b.Property<double>("CreationTime");

                    b.Property<double>("LastPulseTime");

                    b.Property<int>("ParticipatingNodesNum");

                    b.HasKey("ID");

                    b.HasIndex("CreatedByProfileID");

                    b.ToTable("MonitoringSessions");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.NodeClosure", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AncestorID");

                    b.Property<int>("DescendantID");

                    b.Property<int>("Distance");

                    b.HasKey("ID");

                    b.HasIndex("AncestorID");

                    b.HasIndex("DescendantID");

                    b.ToTable("NodesClosureTable");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.NodeTag", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.NtwkNode", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<bool>("OpenPing");

                    b.Property<bool>("OpenSSH");

                    b.Property<bool>("OpenTelnet");

                    b.Property<int?>("ParentID");

                    b.Property<int?>("ParentPort");

                    b.Property<uint>("ip");

                    b.HasKey("ID");

                    b.HasIndex("ParentID");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.Profile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("DepthMonitoring");

                    b.Property<int>("MonitorInterval");

                    b.Property<int>("MonitoringSessionDuration");

                    b.Property<int>("MonitoringStartHour");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<bool>("RealTimePingUIUpdate");

                    b.Property<bool>("StartMonitoringOnLaunch");

                    b.HasKey("ID");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.ProfileSelectedTag", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BindedProfileID");

                    b.Property<int>("Flags");

                    b.Property<int>("TagID");

                    b.HasKey("ID");

                    b.HasIndex("BindedProfileID");

                    b.HasIndex("TagID");

                    b.ToTable("ProfilesTagSelection");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.TagAttachment", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("NodeID");

                    b.Property<int>("TagID");

                    b.HasKey("ID");

                    b.HasIndex("NodeID");

                    b.HasIndex("TagID");

                    b.ToTable("TagAttachments");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.CustomWSBinding", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.NtwkNode", "Node")
                        .WithMany("CustomWebServices")
                        .HasForeignKey("NodeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Data.Model.EFDbModel.CustomWebService", "Service")
                        .WithMany("Bindings")
                        .HasForeignKey("ServiceID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Data.Model.EFDbModel.MonitoringMessage", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.MonitoringPulseResult")
                        .WithMany("Messages")
                        .HasForeignKey("MonitoringPulseResultID");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.MonitoringPulseResult", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.MonitoringSession")
                        .WithMany("Pulses")
                        .HasForeignKey("MonitoringSessionID");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.MonitoringSession", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.Profile", "CreatedByProfile")
                        .WithMany("MonitoringSessions")
                        .HasForeignKey("CreatedByProfileID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Data.Model.EFDbModel.NodeClosure", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.NtwkNode", "Ancestor")
                        .WithMany("Ancestors")
                        .HasForeignKey("AncestorID");

                    b.HasOne("Data.Model.EFDbModel.NtwkNode", "Descendant")
                        .WithMany("Descendants")
                        .HasForeignKey("DescendantID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Data.Model.EFDbModel.NtwkNode", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.NtwkNode", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentID");
                });

            modelBuilder.Entity("Data.Model.EFDbModel.ProfileSelectedTag", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.Profile", "BindedProfile")
                        .WithMany("FilterTagSelection")
                        .HasForeignKey("BindedProfileID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Data.Model.EFDbModel.NodeTag", "Tag")
                        .WithMany("ProfilesFilterSelections")
                        .HasForeignKey("TagID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Data.Model.EFDbModel.TagAttachment", b =>
                {
                    b.HasOne("Data.Model.EFDbModel.NtwkNode", "Node")
                        .WithMany("Tags")
                        .HasForeignKey("NodeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Data.Model.EFDbModel.NodeTag", "Tag")
                        .WithMany("Attachments")
                        .HasForeignKey("TagID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
