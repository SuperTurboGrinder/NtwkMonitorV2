using System;
using System.Collections.Generic;

using Data.Abstract.Converters;
using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;
using Data.Model.Enums;

namespace Data.DataServices.Conversion {

class ViewModelToEFModelConverter : IViewModelToEFModelConverter {

    public EFDbModel.CustomWebService Convert(CustomWebService cws) {
        return new EFDbModel.CustomWebService() {
            ID = cws.ID,
            ServiceName = cws.ServiceName,
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
            SendMonitoringAlarm = profile.SendMonitoringAlarm,
            MonitoringAlarmEmail = profile.MonitoringAlarmEmail,
            StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
            MonitoringStartHour = profile.MonitoringStartHour,
            MonitoringEndHour = profile.MonitoringEndHour,
            MonitorInterval = profile.MonitorInterval,
            DepthMonitoring = profile.DepthMonitoring
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
        MonitoringMessageType mt;
        //should already be validated to convert
        Enum.TryParse(message.MessageType, out mt);
        return new EFDbModel.MonitoringMessage() {
            MessageType = mt,
            MessageSourceNodeName = message.MessageSourceNodeName,
            NumSkippedChildren = message.NumSkippedChildren
        };
    }
}

}