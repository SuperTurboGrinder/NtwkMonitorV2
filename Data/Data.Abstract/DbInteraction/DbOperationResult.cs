using Data.Abstract.Enum;

namespace Data.Abstract.DbInteraction {

public class DbOperationResult<TDbModel> {
    public DbOperationResult(TDbModel result, bool success) {
        Success = success;
        Result = result;
    }
    public bool Success { get; }
    public TDbModel Result { get; }
}

public class DbOperationVoidResult : DbOperationResult<VoidResult> {
    static readonly VoidResult Void = new VoidResult();

    public DbOperationVoidResult(bool success) : base(Void, success) {}
}

}
