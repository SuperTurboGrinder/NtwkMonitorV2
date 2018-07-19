using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;

namespace Data.Abstract.Converters {

interface IEFModelToViewModelConverter {
    CustomWebService Convert(EFDbModel.CustomWebService cws);
    Profile Convert(EFDbModel.Profile profile);
    NtwkNode Convert(EFDbModel.NtwkNode node);
    NodeTag Convert(EFDbModel.NodeTag tag);
    MonitoringSession Convert(EFDbModel.MonitoringSession session);
    MonitoringPulseResult Convert(EFDbModel.MonitoringPulseResult pulse);
    MonitoringMessage Convert(EFDbModel.MonitoringMessage message);
}

}