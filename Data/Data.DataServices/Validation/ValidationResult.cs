
namespace Data.DataServices.Validation {

struct ValidationResult {
    readonly bool IsValid;
    readonly string Error;

    ValidationResult(bool isValid, string error) {
        IsValid = isValid;
        Error = error;
    }

    static ValidationResult _Valid = new ValidationResult(true, null);
    public static ValidationResult Valid {
        get { return _Valid; }
    }

    public static ValidationResult Invalid(string error) {
        ValidationResult result = new ValidationResult(false, error);
        return result;
    }
}

}