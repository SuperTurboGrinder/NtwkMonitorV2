using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Abstract.Validation;
using Data.Abstract.Converters;
using Data.Model.ViewModel;
using EFDbModel = Data.Model.EFDbModel;

namespace Data.DataServices.Services {

public class MonitoringDataService : IMonitoringDataService {
    readonly IDataRepository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;
    readonly CommonServiceUtils utils;

    public MonitoringDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) {
        repo = _repo;
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
        utils = new CommonServiceUtils(repo);
    }
    
    public async Task<DataActionResult<MonitoringSession>> GetNewSession(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActResult<MonitoringSession>(pValidationError);
        }
        DbOperationResult<EFDbModel.MonitoringSession> dbOpResult =
            await repo.GetNewSession(profileID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<MonitoringSession>("Unable to create monitoring session.");
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    public async Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    ) {
        string sValidationError = await utils.ValidateSessionID(sessionID);
        if(sValidationError != null) {
            return utils.FailActResult<MonitoringPulseResult>(sValidationError);
        }
        string messagesValidationError = messages
            .Select(m => validator.Validate(m))
            .FirstOrDefault(err => err != null);
        if(messagesValidationError != null) {
            return utils.FailActResult<MonitoringPulseResult>(messagesValidationError);
        }
        EFDbModel.MonitoringPulseResult convertedPulseResult =
            viewToEFConverter.Convert(pulseResult);
        IEnumerable<EFDbModel.MonitoringMessage> convertedMessages = messages
            .Select(m => viewToEFConverter.Convert(m));
        DbOperationResult<EFDbModel.MonitoringPulseResult> dbOpResult =
            await repo.SavePulseResult(sessionID, convertedPulseResult, convertedMessages);
        if(!dbOpResult.Success) {
            return utils.FailActResult<MonitoringPulseResult>(
                "Unable to save pulse result to database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }
    
    public async Task<DataActionVoidResult> ClearEmptySessions() {
        DbOperationVoidResult dbOpResult =
            await repo.ClearEmptySessions();
        if(!dbOpResult.Success) {
            return utils.FailActVoid("Unable to clear empty sessions from database.");
        }
        return utils.SuccActVoid();
    }

    public async Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActResult<IEnumerable<MonitoringSession>>(pValidationError);
        }
        DbOperationResult<IEnumerable<EFDbModel.MonitoringSession>> dbOpResult =
            await repo.GetSessionsForProfile(profileID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<MonitoringSession>>("Unable to get monitoring sessions for settings profile.");
        }
        return utils.SuccActResult(dbOpResult.Result
            .Select(s => EFToViewConverter.Convert(s))
        );
    }
    
    //includes messages
    public async Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID) {
        string sValidationError = await utils.ValidateSessionID(monitoringSessionID);
        if(sValidationError != null) {
            return utils.FailActResult<IEnumerable<MonitoringPulseResult>>(sValidationError);
        }
        DbOperationResult<IEnumerable<EFDbModel.MonitoringPulseResult>> dbOpResult =
            await repo.GetSessionReport(monitoringSessionID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<MonitoringPulseResult>>("Unable to get monitoring session report from database.");
        }
        IEnumerable<MonitoringPulseResult> convertedPulses = dbOpResult.Result
            .Select(p => {
                IEnumerable<MonitoringMessage> messages = p.Messages
                    .Select(m => EFToViewConverter.Convert(m));
                MonitoringPulseResult pulse = EFToViewConverter.Convert(p);
                pulse.Messages = messages;
                return pulse;
            });
        return utils.SuccActResult(convertedPulses);
    }
}

}