using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;

namespace Data.Abstract.Converters {

interface IViewModelToEFModelConverter {
    EFDbModel.CustomWebService Convert(CustomWebService cws);
    //EFDbModel.Network Convert(Network network);
    EFDbModel.NtwkNode Convert(NtwkNode node);
    EFDbModel.NodeTag Convert(NodeTag tag);
    EFDbModel.MonitoringSession Convert(MonitoringSession session);
    EFDbModel.MonitoringPulseResult Convert(MonitoringPulseResult pulse);
    EFDbModel.MonitoringMessage Convert(MonitoringMessage message);
}

}