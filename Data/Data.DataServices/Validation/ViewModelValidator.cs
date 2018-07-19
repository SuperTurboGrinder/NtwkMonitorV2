using System;
using Data.Model.ViewModel;
using Data.Model.Enums;
using Data.Abstract.Validation;

namespace Data.DataServices.Validation {

class ViewModelValidator : IViewModelValidator {
    public string Validate(
        CustomWebService val
    ) {
        if(!ValidationUtils.IsValidName(val.ServiceName)) {
            return $"Invalid service name.\n({ValidationUtils.NamesFormatMessage}";
        }
        bool hasParam1 = !string.IsNullOrEmpty(val.Parametr1Name);
        bool hasParam2 = !string.IsNullOrEmpty(val.Parametr2Name);
        bool hasParam3 = !string.IsNullOrEmpty(val.Parametr3Name);
        bool AreParameterNamesInOrder =
            !(!hasParam1)?(hasParam2 || hasParam3):(!hasParam2 || hasParam3);
        if(!AreParameterNamesInOrder) {
            return "Paremeter names are not in order.";
        }
        if(hasParam1) {
            Func<int, string> paramNameMessage = (int n) =>
                $"Service paremeter{n} name is invalid.\n({ValidationUtils.NamesFormatMessage})";
            if(ValidationUtils.IsValidName(val.Parametr1Name)) {
                return paramNameMessage(1);
            }
            else if(hasParam2) {
                if(ValidationUtils.IsValidName(val.Parametr2Name)) {
                    return paramNameMessage(2);
                }
                else if(hasParam3) {
                    if(ValidationUtils.IsValidName(val.Parametr3Name)) {
                        return paramNameMessage(3);
                    }
                }
            }
        }
        bool IsServiceStrValid = (
            (val.ServiceStr.StartsWith("http://") || val.ServiceStr.StartsWith("https://")) &&
            ((hasParam1)?(val.ServiceStr.Contains("{param1}")):(
                (hasParam2)?(val.ServiceStr.Contains("{param2}")):(
                    (hasParam3)?(val.ServiceStr.Contains("{param3}")):(true)
                )
            ))
        );
        if(!IsServiceStrValid) {
            return "Invalid service string format.";
        }
        return null;
    }
    
    public string Validate(
        NodeTag val
    ) {
        if(!ValidationUtils.IsValidName(val.Name)) {
            return $"Invalid tag name format.\n({ValidationUtils.NamesFormatMessage})";
        }
        return null;
    }

    public string Validate(
        NtwkNode val
    ) {
        if(ValidationUtils.IsValidName(val.Name)) {
            return $"Invalid node name format.\n({ValidationUtils.NamesFormatMessage})";
        }
        string ipFormatError = null;
        try {
            Data.DataServices.Conversion.ConversionUtils.IPStrToUInt32(val.ipStr);
        }
        catch(ArgumentNullException) {
            ipFormatError = "Ip address string can not be null.";
        }
        catch(FormatException) {
            ipFormatError = "Invalid ip address string format.";
        }
        return ipFormatError;
    }

    public string Validate(Profile profile) {
        if(ValidationUtils.IsValidName(profile.Name)) {
            return $"Invalid profile name format.\n({ValidationUtils.NamesFormatMessage})";
        }
        string emailAddrError = null;
        try {
            var addr = new System.Net.Mail.MailAddress(profile.MonitoringAlarmEmail);
        }
        catch(ArgumentNullException) {
            if(profile.SendMonitoringAlarm) {
                emailAddrError = "Email address not set when monitoring alarm is active.";
            }
            //Otherwise it's ok.
        }
        catch(ArgumentException) {
            emailAddrError = "Email address can not be a blank string.";
        }
        catch(FormatException) {
            return "Invalid email address format.";
        }
        if(emailAddrError != null) {
            return emailAddrError;
        }
        if(profile.MonitoringStartHour >= 0 &&
            profile.MonitoringStartHour < 24
        ) {
            return "Monitoring start hour is out of 0-23 range.";
        }
        if(profile.MonitoringEndHour >= 0 &&
            profile.MonitoringEndHour < 24
        ) {
            return "Monitoring end hour is out of 0-23 range.";
        }
        if(profile.MonitoringStartHour >= profile.MonitoringEndHour) {
            return "Monitoring start hour should be earlier then end hour.";
        }
        return null;
    }

    public string Validate(MonitoringMessage message) {
        MonitoringMessageType mt;
        if(!Enum.TryParse(message.MessageType, out mt)) {
            return "Invalid monitoring message type value.";
        }
        return null;
    }
}

}