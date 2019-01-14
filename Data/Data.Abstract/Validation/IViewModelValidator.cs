using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.Validation
{
//will return error message or null if OK
    public interface IViewModelValidator
    {
        StatusMessage Validate(CustomWebService cws);
        StatusMessage Validate(Profile profile);
        StatusMessage Validate(NtwkNode node);
        StatusMessage Validate(NodeTag tag);
        StatusMessage Validate(MonitoringMessage message);
    }
}