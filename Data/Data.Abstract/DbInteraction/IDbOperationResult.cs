using Data.Abstract.Enum;

namespace Data.Abstract.DbInteraction {

public enum VoidResult {};

public interface IDbOperationResult<TDbModel> {
    bool Success { get; }
    TDbModel Result { get; }
}

public interface IDbOperationVoidResult : IDbOperationResult<VoidResult> {}

}
