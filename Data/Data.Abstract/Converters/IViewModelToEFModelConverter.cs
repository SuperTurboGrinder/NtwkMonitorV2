using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ViewModel;

namespace Data.Abstract.Converters
{
    public interface IViewModelToEfModelConverter
    {
        EFDbModel.CustomWebService Convert(CustomWebService cws);
        EFDbModel.Profile Convert(Profile profile);
        EFDbModel.NtwkNode Convert(NtwkNode node);
        EFDbModel.NodeTag Convert(NodeTag tag);
        EFDbModel.MonitoringPulseResult Convert(MonitoringPulseResult pulse);
        EFDbModel.MonitoringMessage Convert(MonitoringMessage message);
    }
}