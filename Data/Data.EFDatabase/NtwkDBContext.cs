using Microsoft.EntityFrameworkCore;
using Data.Model.EFDbModel;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.EFDatabase
{
// ReSharper disable once IdentifierTypo
// ReSharper disable once InconsistentNaming
    public class NtwkDBContext : DbContext
    {
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<NodeTag> Tags { get; set; }
        public DbSet<CustomWebService> WebServices { get; set; }
        public DbSet<NtwkNode> Nodes { get; set; }
        public DbSet<NodeClosure> NodesClosureTable { get; set; }

        public DbSet<TagAttachment> TagAttachments { get; set; }
        public DbSet<ProfileSelectedTag> ProfilesTagSelection { get; set; }
        public DbSet<CustomWsBinding> WebServiceBindings { get; set; }

        public DbSet<MonitoringSession> MonitoringSessions { get; set; }
        public DbSet<MonitoringPulseResult> MonitoringPulses { get; set; }
        public DbSet<MonitoringMessage> MonitoringMessages { get; set; }

        // ReSharper disable once IdentifierTypo
        public NtwkDBContext(DbContextOptions<NtwkDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .HasOne(pst => pst.BindedProfile)
                .WithMany(p => p.FilterTagSelection)
                .HasForeignKey(pst => pst.BindedProfileID);
            modelBuilder.Entity<ProfileSelectedTag>()
                .HasOne(pvst => pvst.Tag)
                .WithMany(t => t.ProfilesFilterSelections)
                .HasForeignKey(pst => pst.TagID);
            modelBuilder.Entity<CustomWsBinding>()
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