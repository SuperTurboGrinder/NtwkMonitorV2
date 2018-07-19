using Data.Model.ViewModel;

namespace Data.Abstract.Validation {

//will return error message or null if OK
public interface IViewModelValidator {
    string Validate(CustomWebService cws);
    string Validate(Profile profile);
    string Validate(NtwkNode node);
    string Validate(NodeTag tag);
    string Validate(MonitoringMessage message);
}

}