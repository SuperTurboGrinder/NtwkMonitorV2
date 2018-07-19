using System;

namespace Data.Abstract.DbInteraction {

public interface IDbErrorLogger {
    void LogException(Exception ex);
}

}