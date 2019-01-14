using System.Linq;
using Data.Model.ResultsModel;

namespace Data.DataServices.Validation
{
    static class ValidationUtils
    {
        //Names can only include letters, numbers and \"_\" character.

        public static StatusMessage IsValidName(string name)
        {
            return (
                !string.IsNullOrEmpty(name) &&
                name.All(c =>
                    char.IsLetterOrDigit(c)
                    || c == '-' || c == '_' || c == '.'
                )
            )
                ? StatusMessage.Ok
                : StatusMessage.InvalidName;
        }
    }
}