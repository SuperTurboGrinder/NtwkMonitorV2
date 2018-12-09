using System;
using System.Collections.Generic;

using Data.Abstract.Converters;
using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;
using Data.Model.Enums;

namespace Data.DataServices.Conversion {

public class ViewModelToEFModelConverter : IViewModelToEFModelConverter {

    public EFDbModel.CustomWebService Convert(CustomWebService cws) {
        return new EFDbModel.CustomWebService() {
            ID = cws.ID,
            Name = cws.Name,
            ServiceStr = cws.ServiceStr,
            Parametr1Name = cws.Parametr1Name,
            Parametr2Name = cws.Parametr2Name,
            Parametr3Name = cws.Parametr3Name
        };
    }

    public EFDbModel.Profile Convert(Profile profile) {
        return new EFDbModel.Profile() {
            ID = profile.ID,
            Name = profile.Name,
            StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
            MonitoringStartHour = profile.MonitoringStartHour,
            MonitoringSessionDuration = profile.MonitoringSessionDuration,
            MonitorInterval = profile.MonitorInterval,
            DepthMonitoring = profile.DepthMonitoring,
            RealTimePingUIUpdate = profile.RealTimePingUIUpdate
        };
    }
    
    public EFDbModel.NtwkNode Convert(NtwkNode node) {
        return new EFDbModel.NtwkNode() {
            ID = node.ID,
            ParentID = node.ParentID,
            ParentPort = node.ParentPort,
            Name = node.Name,
            ip = Conversion.ConversionUtils.IPStrToUInt32(node.ipStr),
            OpenPing = node.IsOpenPing,
            OpenTelnet = node.IsOpenTelnet,
            OpenSSH = node.IsOpenSSH
        };
    }
    
    public EFDbModel.NodeTag Convert(NodeTag tag) {
        return new EFDbModel.NodeTag() {
            ID = tag.ID,
            Name = tag.Name
        };
    }
    
    public EFDbModel.MonitoringPulseResult Convert(MonitoringPulseResult pulse) {
        return new EFDbModel.MonitoringPulseResult() {
            Responded = pulse.Responded,
            Silent = pulse.Silent,
            Skipped = pulse.Skipped
        };
    }
    
    public EFDbModel.MonitoringMessage Convert(MonitoringMessage message) {
        //should already be validated to convert
        Enum.TryParse(message.MessageType, out MonitoringMessageType mt);
        return new EFDbModel.MonitoringMessage() {
            MessageType = mt,
            MessageSourceNodeName = message.MessageSourceNodeName,
            NumSkippedChildren = message.NumSkippedChildren
        };
    }
}

}