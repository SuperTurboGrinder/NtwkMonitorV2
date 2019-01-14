// ReSharper disable IdentifierTypo

namespace Data.Model.ResultsModel
{
    public enum StatusMessage
    {
        Ok = 0,

        DatabaseInternalError = 101,

        //invalid id messages
        ErrorWhileCheckingIdInDatabase = 201,
        InvalidProfileId = 202,
        InvalidTagId = 203,
        InvalidNodeId = 204,
        InvalidSessionId = 205,
        InvalidTagsIDs = 206,

        //other input validation
        ErrorWhileCheckingNameExistanceInDatabase = 301,
        NameAlreadyClaimed = 302,
        ErrorWhileGetingExistanceOfWsBindingFromDatabase = 303,
        BindingBetweenSpecifiedServiceAndNodeAlreadyExists = 304,
        ErrorWhileCheckingCwsParamNumberInDatabase = 305,
        InvalidWebServiceId = 306,
        ServiceBindingSetParementersValueCanNotBeNull = 307,
        RedundantParameterValuesInServiceBinding = 308,
        ErrorWhileCheckingIfNodeIsPartOfSubtree = 309,
        NodeIsPartOfSpecifiedSubtree = 310,
        BindingBetweenSpecifiedServiceAndNodeDoesNotExist = 311,

        //view model validation
        InvalidName = 401,
        InvalidCwsParam1Name = 402,
        InvalidCwsParam2Name = 403,
        InvalidCwsParam3Name = 404,
        CwsParameterNamesAreNotInOrder = 405,
        InvalidWebServiceStringFormat = 406,
        IpAddressStringIsNull = 407,
        IpAddressStringFormatIsInvalid = 408,
        InvalidMonitoringMessageTypeValue = 409,

        //controllers services errors
        //executables service
        NoSpecifiedExecServiceFoundInBinDirectory = 501,
        TooManyCandidatesForSpecefiedExecServiceInBinDirectory = 502,
        NotEnoughOsRightsToSearchForExecServices = 503,
        CouldNotFindBinDirectoryWhileSearchingForExecutableServices = 504,
        UnknownIoErrorWhileTryingToFindExecutableServices = 505,
        ExecutableServiceStartError = 506,

        //ping service
        PingExecutionServiceError = 507,

        //web services launcher service
        CustomWebServiceStartError = 508,
    }

    public static class StatusMessageExt
    {
        public static bool Failure(this StatusMessage message) =>
            message != StatusMessage.Ok;
    }
}