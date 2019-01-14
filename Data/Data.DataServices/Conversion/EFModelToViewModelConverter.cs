using Data.Abstract.Converters;
using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;

namespace Data.DataServices.Conversion
{
    public class EfModelToViewModelConverter : IEfModelToViewModelConverter
    {
        public CustomWebService Convert(EFDbModel.CustomWebService cws)
        {
            return new CustomWebService()
            {
                Id = cws.ID,
                Name = cws.Name,
                ServiceStr = cws.ServiceStr,
                Parameter1Name = cws.Parametr1Name,
                Parameter2Name = cws.Parametr2Name,
                Parameter3Name = cws.Parametr3Name
            };
        }

        public Profile Convert(EFDbModel.Profile profile)
        {
            return new Profile()
            {
                Id = profile.ID,
                Name = profile.Name,
                StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
                MonitorInterval = profile.MonitorInterval,
                DepthMonitoring = profile.DepthMonitoring,
                RealTimePingUiUpdate = profile.RealTimePingUIUpdate
            };
        }

        public NtwkNode Convert(EFDbModel.NtwkNode node)
        {
            return new NtwkNode()
            {
                Id = node.ID,
                ParentId = node.ParentID,
                ParentPort = node.ParentPort,
                Name = node.Name,
                IpStr = ConversionUtils.UInt32ToIpStr(node.ip),
                IsOpenPing = node.OpenPing,
                IsOpenTelnet = node.OpenTelnet,
                IsOpenSsh = node.OpenSSH
            };
        }

        public NodeTag Convert(EFDbModel.NodeTag tag)
        {
            return new NodeTag()
            {
                Id = tag.ID,
                Name = tag.Name
            };
        }

        public MonitoringSession Convert(EFDbModel.MonitoringSession session)
        {
            return new MonitoringSession()
            {
                Id = session.ID,
                CreatedByProfileId = session.CreatedByProfileID,
                ParticipatingNodesNum = session.ParticipatingNodesNum,
                CreationTime = session.CreationTime,
                LastPulseTime = session.LastPulseTime
            };
        }

        public MonitoringPulseResult Convert(EFDbModel.MonitoringPulseResult pulse)
        {
            return new MonitoringPulseResult()
            {
                CreationTime = pulse.CreationTime,
                Responded = pulse.Responded,
                Silent = pulse.Silent,
                Skipped = pulse.Skipped
            };
        }

        public MonitoringMessage Convert(EFDbModel.MonitoringMessage message)
        {
            return new MonitoringMessage()
            {
                MessageType = message.MessageType.ToString(),
                MessageSourceNodeName = message.MessageSourceNodeName,
                NumSkippedChildren = message.NumSkippedChildren
            };
        }
    }
}