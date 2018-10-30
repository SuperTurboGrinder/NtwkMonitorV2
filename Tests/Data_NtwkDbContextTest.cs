using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System.Linq;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.EFDatabase;
using Data.EFDatabase.Logic;
using static Tests.EFDatabaseMockingUtils;

namespace Tests {

public class Data_NtwkDbContextTest {

    // test basic database structure soundness
    [Fact]
    public void ContextRelationshipsAreCorrect() {
        var (context, _, _) = GetTestDataContext();

        var profile = context.Profiles
            .First();

        var nodes = context.Nodes
            .Include(n => n.Parent)
            .Include(n => n.Children)
            .Include(n => n.Tags)
                .ThenInclude(t => t.Node)
            .Include(n => n.Tags)
                .ThenInclude(t => t.Tag)
            .Include(n => n.CustomWebServices)
                .ThenInclude(s => s.Node)
            .Include(n => n.CustomWebServices)
                .ThenInclude(s => s.Service)
            .ToList();
        
        var monitoringSession = context.MonitoringSessions
            .Include(s => s.CreatedByProfile)
            .Include(s => s.Pulses)
                .ThenInclude(p => p.Messages)
            .Single();

        Assert.Equal(TestProfileName, profile.Name);
        Assert.Null(nodes[0].Parent);
        Assert.Equal(nodes[0], nodes[1].Parent);
        Assert.Single(nodes[0].Children);
        Assert.Equal(nodes[1], nodes[0].Children.First());
        Assert.Empty(nodes[1].Children);
        Assert.Single(nodes[0].Tags);
        Assert.Equal(2, nodes[1].Tags.Count);
        Assert.Equal(TagNames[0], nodes[0].Tags.First().Tag.Name);
        Assert.Equal(TagNames[0], nodes[1].Tags.First().Tag.Name);
        Assert.Equal(TagNames[1], nodes[1].Tags.Skip(1).First().Tag.Name);
        Assert.Equal(2, nodes[0].CustomWebServices.Count());
        Assert.Equal(2, nodes[1].CustomWebServices.Count());
        Assert.Single(nodes[2].CustomWebServices);
        Assert.Equal(WebServicesNames[0], nodes[0].CustomWebServices.First().Service.Name);
        Assert.Equal(WebServicesNames[0], nodes[1].CustomWebServices.First().Service.Name);

        Assert.Equal(2, monitoringSession.Pulses.Count);
        Assert.Empty(monitoringSession.Pulses.First().Messages);
        Assert.Single(monitoringSession.Pulses.Skip(1).First().Messages);
        Assert.Equal(NodeNames[0], monitoringSession.Pulses.Skip(1).First().Messages.First().MessageSourceNodeName);
    }
}

}
