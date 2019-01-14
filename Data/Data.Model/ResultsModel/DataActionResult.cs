namespace Data.Model.ResultsModel
{
    public class DataActionResult<TModel>
    {
        DataActionResult(
            TModel convertedDbOpResult,
            StatusMessage statusMessage = StatusMessage.Ok
        )
        {
            Result = convertedDbOpResult;
            Status = statusMessage;
        }

        public static DataActionResult<TModel> Successful(TModel result)
        {
            return new DataActionResult<TModel>(result);
        }

        public static DataActionResult<TModel> Failed(StatusMessage status)
        {
            return new DataActionResult<TModel>(default(TModel), status);
        }

        public readonly StatusMessage Status;
        public readonly TModel Result;
    }
}