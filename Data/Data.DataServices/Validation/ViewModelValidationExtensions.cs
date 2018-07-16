using System;
using Data.Model.ViewModel;

namespace Data.DataServices.Validation {

static class ViewModelValidationExtensions {
    static ValidationResult GetValidation(
        this CustomWebService val
    ) {
        if(!ValidationUtils.IsValidName(val.ServiceName)) {
            return ValidationResult.Invalid($"Invalid service name.\n({ValidationUtils.NamesFormatMessage}");
        }
        bool hasParam1 = !string.IsNullOrEmpty(val.Parametr1Name);
        bool hasParam2 = !string.IsNullOrEmpty(val.Parametr2Name);
        bool hasParam3 = !string.IsNullOrEmpty(val.Parametr3Name);
        bool AreParameterNamesInOrder =
            !(!hasParam1)?(hasParam2 || hasParam3):(!hasParam2 || hasParam3);
        if(!AreParameterNamesInOrder) {
            return ValidationResult.Invalid("Paremeter names are not in order.");
        }
        if(hasParam1) {
            Func<int, string> paramNameMessage = (int n) =>
                $"Service paremeter{n} name is invalid.\n({ValidationUtils.NamesFormatMessage})";
            if(ValidationUtils.IsValidName(val.Parametr1Name)) {
                return ValidationResult.Invalid(paramNameMessage(1));
            }
            else if(hasParam2) {
                if(ValidationUtils.IsValidName(val.Parametr2Name)) {
                    return ValidationResult.Invalid(paramNameMessage(2));
                }
                else if(hasParam3) {
                    if(ValidationUtils.IsValidName(val.Parametr3Name)) {
                        return ValidationResult.Invalid(paramNameMessage(3));
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
            return ValidationResult.Invalid("Invalid service string format.");
        }
        return ValidationResult.Valid;
    }
    
    static ValidationResult GetValidation(
        this NodeTag val
    ) {
        if(!ValidationUtils.IsValidName(val.Name)) {
            return ValidationResult.Invalid($"Invalid tag name format.\n({ValidationUtils.NamesFormatMessage})");
        }
        return ValidationResult.Valid;
    }

    static ValidationResult GetValidation(
        this NtwkNode val
    ) {
        if(ValidationUtils.IsValidName(val.Name)) {
            return ValidationResult.Invalid($"Invalid node name format.\n({ValidationUtils.NamesFormatMessage})");
        }
        return ValidationResult.Valid;
    }
}

}