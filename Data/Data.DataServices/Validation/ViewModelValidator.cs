using System;
using Data.Model.ViewModel;
using Data.Model.Enums;
using Data.Model.ResultsModel;
using Data.Abstract.Validation;

namespace Data.DataServices.Validation {

public class ViewModelValidator : IViewModelValidator {
    public StatusMessage Validate(CustomWebService val) {
        StatusMessage nameValidationStatus =
            ValidationUtils.IsValidName(val.ServiceName);
        if(nameValidationStatus.Failure())
            return nameValidationStatus;
        bool hasParam1 = !string.IsNullOrEmpty(val.Parametr1Name);
        bool hasParam2 = !string.IsNullOrEmpty(val.Parametr2Name);
        bool hasParam3 = !string.IsNullOrEmpty(val.Parametr3Name);
        bool AreParameterNamesInOrder =
            hasParam3
                ? hasParam1 && hasParam2
                : hasParam2
                    ? hasParam1
                    : true;
        if(!AreParameterNamesInOrder) {
            return StatusMessage.CWSParameterNamesAreNotInOrder;
        }
        StatusMessage paramNamesValidation =
            hasParam1 && ValidationUtils.IsValidName(val.Parametr1Name).Failure()
                ? StatusMessage.InvalidCWSParam1Name
                : hasParam2 &&  ValidationUtils.IsValidName(val.Parametr2Name).Failure()
                    ? StatusMessage.InvalidCWSParam2Name
                    : hasParam3 && ValidationUtils.IsValidName(val.Parametr3Name).Failure()
                        ? StatusMessage.InvalidCWSParam3Name
                        : StatusMessage.Ok;
        if(paramNamesValidation.Failure())
            return paramNamesValidation;
        bool IsServiceStrValid = (
            (val.ServiceStr.StartsWith("http://") || val.ServiceStr.StartsWith("https://")) &&
            (hasParam1)
        )
            ? val.ServiceStr.Contains("{param1}")
            : hasParam2
                ? val.ServiceStr.Contains("{param2}")
                : hasParam3
                    ? val.ServiceStr.Contains("{param3}")
                    : true;
        return (!IsServiceStrValid)
            ? StatusMessage.InvalidWebServiceStringFormat
            : StatusMessage.Ok;
    }
    
    public StatusMessage Validate(NodeTag val) =>
        ValidationUtils.IsValidName(val.Name);

    public StatusMessage Validate(NtwkNode val) {
        StatusMessage nameValidationStatus = ValidationUtils.IsValidName(val.Name);
        if(nameValidationStatus.Failure()) {
            return nameValidationStatus;
        }
        StatusMessage ipFormatStatus = StatusMessage.Ok;
        try {
            Data.DataServices.Conversion.ConversionUtils.IPStrToUInt32(val.ipStr);
        }
        catch(ArgumentNullException) {
            ipFormatStatus = StatusMessage.IpAddressStringIsNull;
        }
        catch(FormatException) {
            ipFormatStatus = StatusMessage.IpAddressStringFormatIsInvalid;
        }
        return ipFormatStatus;
    }

    public StatusMessage Validate(Profile profile) {
        const int MIDNIGHT = 0;
        const int TWENTY_FOUR_HOURS = 24;
        StatusMessage nameValidationStatus = 
            ValidationUtils.IsValidName(profile.Name);
        if(nameValidationStatus.Failure()) {
            return nameValidationStatus;
        }
        if(profile.MonitoringStartHour < MIDNIGHT ||
            profile.MonitoringStartHour > MIDNIGHT+23
        ) {
            return StatusMessage.MonitoringStartHourIsOutOfRange;
        }
        if(profile.MonitoringSessionDuration < 1 ||
            profile.MonitoringSessionDuration > TWENTY_FOUR_HOURS
        ) {
            return StatusMessage.MonitoringSessionDurationIsOutOfRange;
        }
        return StatusMessage.Ok;
    }

    public StatusMessage Validate(MonitoringMessage message) {
        MonitoringMessageType mt;
        if(!Enum.TryParse(message.MessageType, out mt)) {
            return StatusMessage.InvalidMonitoringMessageTypeValue;
        }
        return StatusMessage.Ok;
    }
}

}