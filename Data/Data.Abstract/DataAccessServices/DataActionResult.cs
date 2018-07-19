using Data.Abstract.Enum;
using Data.Abstract.DbInteraction;

namespace Data.Abstract.DataAccessServices {

public class DataActionResult<TViewModel> {
    public DataActionResult(
        TViewModel convertedDbOpResult,
        bool success,
        string errorString
    ) {
        Result = convertedDbOpResult;
        Success = success;
        Error = errorString;
    }

    public bool Success { get; }
    public string Error { get; }
    public TViewModel Result { get; }
}

public class DataActionVoidResult : DataActionResult<VoidResult> {
    static readonly VoidResult Void = new VoidResult();

    DataActionVoidResult(
        bool success,
        string errorString
    ) : base(Void, success, errorString) {}
}

}
