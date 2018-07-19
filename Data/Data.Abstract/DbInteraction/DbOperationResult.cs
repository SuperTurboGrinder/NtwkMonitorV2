using Data.Abstract.Enum;

namespace Data.Abstract.DbInteraction {

public enum VoidResult {};

public class DbOperationResult<TDbModel> {
    public DbOperationResult(TDbModel result, bool success) {
        Success = success;
        Result = result;
    }
    public bool Success { get; }
    public TDbModel Result { get; }
}

public class DbOperationVoidResult : DbOperationResult<VoidResult> {
    public DbOperationVoidResult(bool success) : base(new VoidResult(), success) {}
}

}
