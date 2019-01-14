using System;
using Data.Model.ViewModel;
using Data.Model.Enums;
using Data.Model.ResultsModel;
using Data.Abstract.Validation;

namespace Data.DataServices.Validation
{
    public class ViewModelValidator : IViewModelValidator
    {
        public StatusMessage Validate(CustomWebService val)
        {
            StatusMessage nameValidationStatus =
                ValidationUtils.IsValidName(val.Name);
            if (nameValidationStatus.Failure())
                return nameValidationStatus;
            bool hasParam1 = val.Parameter1Name != null;
            bool hasParam2 = val.Parameter2Name != null;
            bool hasParam3 = val.Parameter3Name != null;
            bool areParameterNamesInOrder =
                hasParam3
                    ? hasParam1 && hasParam2
                    : !hasParam2 || hasParam1;
            if (!areParameterNamesInOrder)
            {
                return StatusMessage.CwsParameterNamesAreNotInOrder;
            }

            StatusMessage paramNamesValidation =
                hasParam1 && ValidationUtils.IsValidName(val.Parameter1Name).Failure()
                    ? StatusMessage.InvalidCwsParam1Name
                    : hasParam2 && ValidationUtils.IsValidName(val.Parameter2Name).Failure()
                        ? StatusMessage.InvalidCwsParam2Name
                        : hasParam3 && ValidationUtils.IsValidName(val.Parameter3Name).Failure()
                            ? StatusMessage.InvalidCwsParam3Name
                            : StatusMessage.Ok;
            if (paramNamesValidation.Failure())
                return paramNamesValidation;
            bool isServiceStrStartHttp = (
                (val.ServiceStr.StartsWith("http://") || val.ServiceStr.StartsWith("https://"))
            );
            bool containsP1Str = val.ServiceStr.Contains("{param1}");
            bool containsP2Str = val.ServiceStr.Contains("{param2}");
            bool containsP3Str = val.ServiceStr.Contains("{param3}");
            bool isParam1Valid = (hasParam1 && containsP1Str)
                                 || (!hasParam1 && !containsP1Str);
            bool isParam2Valid = (hasParam1 && hasParam2 && containsP2Str)
                                 || (!hasParam2 && !containsP2Str);
            bool isParam3Valid = (hasParam1 && hasParam2 && hasParam3 && containsP3Str)
                                 || (!hasParam3 && !containsP3Str);
            bool areServiceStrParametersValid =
                isParam1Valid && isParam2Valid && isParam3Valid;
            bool lengthCheck = hasParam1 || val.ServiceStr.Length > 10;
            bool isServiceStrValid = isServiceStrStartHttp
                                     && areServiceStrParametersValid
                                     && lengthCheck;
            /*throw new Exception($@"
                HttpStart: {IsServiceStrStartHttp},
                HasParam: {hasParam1}, {hasParam2}, {hasParam3},
                ContainsParamStr: {containsP1Str}, {containsP2Str}, {containsP3Str},
                ParamValid: {isParam1Valid}, {isParam2Valid}, {isParam3Valid},
                LengthCheck: {LengthCheck},
                IsStrValid: {IsServiceStrValid}
            ");*/
            return (!isServiceStrValid)
                ? StatusMessage.InvalidWebServiceStringFormat
                : StatusMessage.Ok;
        }

        public StatusMessage Validate(NodeTag val) =>
            ValidationUtils.IsValidName(val.Name);

        public StatusMessage Validate(NtwkNode val)
        {
            StatusMessage nameValidationStatus = ValidationUtils.IsValidName(val.Name);
            if (nameValidationStatus.Failure())
            {
                return nameValidationStatus;
            }

            StatusMessage ipFormatStatus = StatusMessage.Ok;
            try
            {
                Conversion.ConversionUtils.IpStrToUInt32(val.IpStr);
            }
            catch (ArgumentNullException)
            {
                ipFormatStatus = StatusMessage.IpAddressStringIsNull;
            }
            catch (FormatException)
            {
                ipFormatStatus = StatusMessage.IpAddressStringFormatIsInvalid;
            }

            return ipFormatStatus;
        }

        public StatusMessage Validate(Profile profile)
        {
            StatusMessage nameValidationStatus =
                ValidationUtils.IsValidName(profile.Name);
            if (nameValidationStatus.Failure())
            {
                return nameValidationStatus;
            }

            return StatusMessage.Ok;
        }

        public StatusMessage Validate(MonitoringMessage message)
        {
            // ReSharper disable once NotAccessedVariable
            MonitoringMessageType mt;
            return !Enum.TryParse(message.MessageType, out mt)
                ? StatusMessage.InvalidMonitoringMessageTypeValue
                : StatusMessage.Ok;
        }
    }
}