using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

using Data.Model.EFDbModel;

namespace Data.EFDatabase {

public class NtwkDBContext : DbContext {
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<NodeTag> Tags { get; set; }
    public DbSet<CustomWebService> WebServices { get; set; }
    public DbSet<NtwkNode> Nodes { get; set; }
    public DbSet<NodeClosure> NodesClosureTable { get; set; }
    
    public DbSet<TagAttachment> TagAttachments { get; set; }
    public DbSet<ProfileSelectedTag> ProfilesTagSelection { get; set; }
    public DbSet<CustomWSBinding> WebServiceBindings { get; set; }

    public DbSet<MonitoringSession> MonitoringSessions { get; set; }
    public DbSet<MonitoringPulseResult> MonitoringPulses { get; set; }
    public DbSet<MonitoringMessage> MonitoringMessages { get; set; }

    public NtwkDBContext(DbContextOptions<NtwkDBContext> options) : base(options) {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<NodeClosure>()
            .HasOne(c => c.Ancestor)
            .WithMany(n => n.Ancestors)
            .HasForeignKey(c => c.AncestorID);
        modelBuilder.Entity<NodeClosure>()
            .HasOne(c => c.Descendant)
            .WithMany(n => n.Descendants)
            .HasForeignKey(c => c.DescendantID);
        modelBuilder.Entity<NtwkNode>()
            .HasOne(n => n.Parent)
            .WithMany(n => n.Children)
            .HasForeignKey(n => n.ParentID);
        modelBuilder.Entity<NtwkNode>()
            .HasMany(n => n.CustomWebServices)
            .WithOne(wsb => wsb.Node)
            .HasForeignKey(wsb => wsb.NodeID);
        modelBuilder.Entity<TagAttachment>()
            .HasOne(a => a.Tag)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TagID);
        modelBuilder.Entity<TagAttachment>()
            .HasOne(a => a.Node)
            .WithMany(n => n.Tags)
            .HasForeignKey(a => a.NodeID);
        modelBuilder.Entity<ProfileSelectedTag>()
            .HasOne(pvst => pvst.BindedProfile)
            .WithMany(p => p.FilterTagSelection)
            .HasForeignKey(pvst => pvst.BindedProfileID);
        modelBuilder.Entity<ProfileSelectedTag>()
            .HasOne(pvst => pvst.Tag)
            .WithMany(t => t.ProfilesFilterSelections)
            .HasForeignKey(pvst => pvst.TagID);
        modelBuilder.Entity<CustomWSBinding>()
            .HasOne(wsb => wsb.Service)
            .WithMany(cws => cws.Bindings)
            .HasForeignKey(wsb => wsb.ServiceID);
        modelBuilder.Entity<MonitoringSession>()
            .HasOne(s => s.CreatedByProfile)
            .WithMany(p => p.MonitoringSessions)
            .HasForeignKey(s => s.CreatedByProfileID);
    }
}

}