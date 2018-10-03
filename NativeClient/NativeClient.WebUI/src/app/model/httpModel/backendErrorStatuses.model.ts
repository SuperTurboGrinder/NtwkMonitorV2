export enum BackendErrorStatuses {
    DatabaseInternalError = 101,

    //invalid id messages
    ErrorWhileCheckingIDInDatabase = 201,
    InvalidProfileID = 202,
    InvalidTagID = 203,
    InvalidNodeID = 204,
    InvalidSessionID = 205,
    InvalidTagsIDs = 206,

    //other input validation
    ErrorWhileCheckingNameExistanceInDatabase = 301,
    NameAlreadyClaimed = 302,
    ErrorWhileGetingExistanceOfWSBindingFromDatabase = 303,
    BindingBetweenSpecifiedServiceAndNodeAlreadyExists = 304,
    ErrorWhileCheckingCWSParamNumberInDatabase = 305,
    InvalidWebServiceID = 306,
    ServiceBindingSetParementersValueCanNotBeNull = 307,
    RedundantParameterValuesInServiceBinding = 308,
    ErrorWhileCheckingIfNodeIsPartOfSubtree = 309,
    NodeIsPartOfSpecifiedSubtree = 310,
    BindingBetweenSpecifiedServiceAndNodeDoesNotExist = 311,

    //view model validation
    InvalidName = 401,
    InvalidCWSParam1Name = 402,
    InvalidCWSParam2Name = 403,
    InvalidCWSParam3Name = 404,
    CWSParameterNamesAreNotInOrder = 405,
    InvalidWebServiceStringFormat = 406,
    IpAddressStringIsNull = 407,
    IpAddressStringFormatIsInvalid = 408,
    EmailAddresIsSetToNull = 409,
    EmailAddressIsABlankString = 410,
    EmailAddressFormatIsInvalid = 411,
    EmailAddressIsNotNullWhileSendAlarmIsNotActive = 412,
    MonitoringStartHourIsOutOfRange = 413,
    MonitoringEndHourIsOutOfRange = 414,
    MonitoringStartHourCanNotBeLaterThanEndHour = 415,
    InvalidMonitoringMessageTypeValue = 416,

    //controllers services errors
    //exceutables service
    NoSpecifiedExecServiceFoundInBinDirectory = 501,
    TooManyCandidatesForSpecefiedExecServiceInBinDirectory = 502,
    NotEnoughOSRightsToSearchForExecServices = 503,
    CouldNotFindBinDirectoryWhileSearchingForExecutableServices = 504,
    UnknownIOErrorWhileTryingToFindExecutableServices = 505,
    ExecutableServiceStartError = 506,
    //ping service
    PingExecutionServiceError = 507,
    //web services launcher service
    CustomWebServiceStartError = 508,
}