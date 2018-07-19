using Data.Abstract.Converters;
using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;

namespace Data.DataServices.Conversion {

class EFModelToViewModelConverter : IEFModelToViewModelConverter {

    public CustomWebService Convert(EFDbModel.CustomWebService cws) {
        return new CustomWebService() {
            ID = cws.ID,
            ServiceName = cws.ServiceName,
            ServiceStr = cws.ServiceStr,
            Parametr1Name = cws.Parametr1Name,
            Parametr2Name = cws.Parametr2Name,
            Parametr3Name = cws.Parametr3Name
        };
    }

    public Profile Convert(EFDbModel.Profile profile) {
        return new Profile() {
            ID = profile.ID,
            Name = profile.Name,
            SendMonitoringAlarm = profile.SendMonitoringAlarm,
            MonitoringAlarmEmail = profile.MonitoringAlarmEmail,
            StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
            MonitoringStartHour = profile.MonitoringStartHour,
            MonitoringEndHour = profile.MonitoringEndHour,
            MonitorInterval = profile.MonitorInterval,
            DepthMonitoring = profile.DepthMonitoring
        };
    }
    
    public NtwkNode Convert(EFDbModel.NtwkNode node) {
        return new NtwkNode() {
            ID = node.ID,
            ParentID = node.ParentID,
            ParentPort = node.ParentPort,
            Name = node.Name,
            ipStr = Conversion.ConversionUtils.UInt32ToIPStr(node.ip),
            IsOpenPing = node.OpenPing,
            IsOpenTelnet = node.OpenTelnet,
            IsOpenSSH = node.OpenSSH
        };
    }
    
    public NodeTag Convert(EFDbModel.NodeTag tag) {
        return new NodeTag() {
            ID = tag.ID,
            Name = tag.Name
        };
    }

    public MonitoringSession Convert(EFDbModel.MonitoringSession session) {
        return new MonitoringSession() {
            ID = session.ID,
            CreatedByProfileID = session.CreatedByProfileID,
            ParticipatingNodesNum = session.ParticipatingNodesNum,
            CreationTime = ConversionUtils.DateTimeFromJSDateTime(
                session.CreationTime
            ),
            LastPulseTime = ConversionUtils.DateTimeFromJSDateTime(
                session.LastPulseTime
            )
        };
    }
    
    public MonitoringPulseResult Convert(EFDbModel.MonitoringPulseResult pulse) {
        return new MonitoringPulseResult() {
            CreationTime = ConversionUtils.DateTimeFromJSDateTime(
                pulse.CreationTime
            ),
            Responded = pulse.Responded,
            Silent = pulse.Silent,
            Skipped = pulse.Skipped
        };
    }
    
    public MonitoringMessage Convert(EFDbModel.MonitoringMessage message) {
        return new MonitoringMessage() {
            MessageType = nameof(message.MessageType),
            MessageSourceNodeName = message.MessageSourceNodeName,
            NumSkippedChildren = message.NumSkippedChildren
        };
    }
}

}