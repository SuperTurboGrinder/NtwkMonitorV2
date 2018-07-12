using Data.Abstract.Enum;

namespace Data.Abstract.DataAccessServices {

public interface IDataActionResult<TModel> {
    bool Success { get; }
    string Error { get; }
    TModel Result { get; }
}

public interface IDataActionVoidResult : IDataActionResult<VoidResult> {}

}
