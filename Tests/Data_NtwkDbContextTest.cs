using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System.Linq;

using Data.Model.EFDbModel;
using Data.Model.Enums;
using Data.EFDatabase;
using Data.EFDatabase.Logic;

namespace Tests {

public class Data_NtwkDbContextTest {

    // test basic database structure soundness
    [Fact]
    public void ContextRelationshipsAreCorrect() {
        EFDatabaseMockingUtils utils = new EFDatabaseMockingUtils();
        var (context, _, _) = utils.GetTestDataContext();

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

        Assert.Equal(utils.TestProfileName, profile.Name);
        Assert.Null(nodes[0].Parent);
        Assert.Equal(nodes[0], nodes[1].Parent);
        Assert.Single(nodes[0].Children);
        Assert.Equal(nodes[1], nodes[0].Children.First());
        Assert.Empty(nodes[1].Children);
        Assert.Single(nodes[0].Tags);
        Assert.Equal(2, nodes[1].Tags.Count);
        Assert.Equal(utils.FirstTagName, nodes[0].Tags.First().Tag.Name);
        Assert.Equal(utils.FirstTagName, nodes[1].Tags.First().Tag.Name);
        Assert.Equal(utils.SecondTagName, nodes[1].Tags.Skip(1).First().Tag.Name);
        Assert.Single(nodes[0].CustomWebServices);
        Assert.Single(nodes[1].CustomWebServices);
        Assert.Equal(utils.WebInterfaceOn8080Name, nodes[0].CustomWebServices.First().Service.Name);
        Assert.Equal(utils.WebInterfaceOn8080Name, nodes[1].CustomWebServices.First().Service.Name);

        Assert.Equal(2, monitoringSession.Pulses.Count);
        Assert.Empty(monitoringSession.Pulses.First().Messages);
        Assert.Single(monitoringSession.Pulses.Skip(1).First().Messages);
        Assert.Equal(utils.FirstNodeName, monitoringSession.Pulses.Skip(1).First().Messages.First().MessageSourceNodeName);
    }
}

}
