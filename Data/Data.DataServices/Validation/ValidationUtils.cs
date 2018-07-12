using System;
using System.Linq;

using Data.Model.Enums;

namespace Data.Model.ViewModel {

static class ValidationUtils {
    public const string NamesFormatMessage = "Names can only include letters, numbers and \"_\" character.";

    public static bool IsValidName(string name) {
        return (!string.IsNullOrEmpty(name)) &&
            name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
    }

    public static MonitoringMessageType? MessageTypeFromValueOrNull(int value) {
        if(IsValidEnumTypeValue<MonitoringMessageType>(value)) {
            return (MonitoringMessageType)value;
        }
        return null;
    }

    private static bool IsValidEnumTypeValue<TEnum>(int value) {
        int valueRange = System.Enum.GetValues(typeof(TEnum)).Length;
        if(value >= 0 && value < valueRange) {
            return true;
        }
        return false;
    }
}

}