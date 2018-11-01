using System;
using System.Linq;

using Data.Model.Enums;
using Data.Model.ResultsModel;

namespace Data.DataServices.Validation {

static class ValidationUtils {
    //Names can only include letters, numbers and \"_\" character.

    public static StatusMessage IsValidName(string name) {
        return (
            !string.IsNullOrEmpty(name)) &&
            name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.'
        )
            ? StatusMessage.Ok
            : StatusMessage.InvalidName;
    }

    public static MonitoringMessageType? MessageTypeFromValueOrNull(int value) {
        if(IsValidEnumTypeValue<MonitoringMessageType>(value)) {
            return (MonitoringMessageType)value;
        }
        return null;
    }

    private static bool IsValidEnumTypeValue<TEnum>(int value) {
        int valueRange = System.Enum.GetValues(typeof(TEnum)).Length;
        return value >= 0 && value < valueRange;
    }
}

}