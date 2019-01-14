using System;
using Data.Abstract.Converters;
using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;
using Data.Model.Enums;

namespace Data.DataServices.Conversion
{
    public class ViewModelToEfModelConverter : IViewModelToEfModelConverter
    {
        public EFDbModel.CustomWebService Convert(CustomWebService cws)
        {
            return new EFDbModel.CustomWebService()
            {
                ID = cws.Id,
                Name = cws.Name,
                ServiceStr = cws.ServiceStr,
                Parametr1Name = cws.Parameter1Name,
                Parametr2Name = cws.Parameter2Name,
                Parametr3Name = cws.Parameter3Name
            };
        }

        public EFDbModel.Profile Convert(Profile profile)
        {
            return new EFDbModel.Profile()
            {
                ID = profile.Id,
                Name = profile.Name,
                StartMonitoringOnLaunch = profile.StartMonitoringOnLaunch,
                MonitorInterval = profile.MonitorInterval,
                DepthMonitoring = profile.DepthMonitoring,
                RealTimePingUIUpdate = profile.RealTimePingUiUpdate
            };
        }

        public EFDbModel.NtwkNode Convert(NtwkNode node)
        {
            return new EFDbModel.NtwkNode()
            {
                ID = node.Id,
                ParentID = node.ParentId,
                ParentPort = node.ParentPort,
                Name = node.Name,
                ip = ConversionUtils.IpStrToUInt32(node.IpStr),
                OpenPing = node.IsOpenPing,
                OpenTelnet = node.IsOpenTelnet,
                OpenSSH = node.IsOpenSsh
            };
        }

        public EFDbModel.NodeTag Convert(NodeTag tag)
        {
            return new EFDbModel.NodeTag()
            {
                ID = tag.Id,
                Name = tag.Name
            };
        }

        public EFDbModel.MonitoringPulseResult Convert(MonitoringPulseResult pulse)
        {
            return new EFDbModel.MonitoringPulseResult()
            {
                Responded = pulse.Responded,
                Silent = pulse.Silent,
                Skipped = pulse.Skipped
            };
        }

        public EFDbModel.MonitoringMessage Convert(MonitoringMessage message)
        {
            //should already be validated to convert
            Enum.TryParse(message.MessageType, out MonitoringMessageType mt);
            return new EFDbModel.MonitoringMessage()
            {
                MessageType = mt,
                MessageSourceNodeName = message.MessageSourceNodeName,
                NumSkippedChildren = message.NumSkippedChildren
            };
        }
    }
}