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
using Data.Model.ResultsModel;

namespace Data.DataServices.Services {

public class MonitoringDataService
    : BaseDataService, IMonitoringDataService {
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public MonitoringDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) : base(_repo) {
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }
    
    public async Task<DataActionResult<MonitoringSession>> GetNewSession(
        int profileID
    ) {
        StatusMessage profIDValidationStatus =
            await ValidateProfileID(profileID);
        if(profIDValidationStatus.Failure()) {
            return DataActionResult<MonitoringSession>.Failed(profIDValidationStatus);
        }
        return FailOrConvert(
            await repo.GetNewSession(profileID),
            session => EFToViewConverter.Convert(session)
        );
    }

    public async Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    ) {
        StatusMessage sessionIDValidationStatus =
            await ValidateSessionID(sessionID);
        if(sessionIDValidationStatus.Failure()) {
            return DataActionResult<MonitoringPulseResult>.Failed(sessionIDValidationStatus);
        }
        StatusMessage messageValidationStatus = StatusMessage.Ok;
        foreach(MonitoringMessage message in messages) {
            messageValidationStatus = validator.Validate(message);
            if(messageValidationStatus.Failure()) {
                return DataActionResult<MonitoringPulseResult>.Failed(
                    messageValidationStatus
                );
            }
        }
        EFDbModel.MonitoringPulseResult convertedPulseResult =
            viewToEFConverter.Convert(pulseResult);
        IEnumerable<EFDbModel.MonitoringMessage> convertedMessages = messages
            .Select(m => viewToEFConverter.Convert(m));
        return FailOrConvert(
            await repo.SavePulseResult(sessionID, convertedPulseResult, convertedMessages),
            pr => EFToViewConverter.Convert(pr)
        );
    }
    
    public async Task<StatusMessage> ClearEmptySessions() {
        return await repo.ClearEmptySessions();
    }

    public async Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID) {
        StatusMessage profIDValidationStatus =
            await ValidateProfileID(profileID);
        if(profIDValidationStatus.Failure()) {
            return DataActionResult<IEnumerable<MonitoringSession>>.Failed(
                profIDValidationStatus
            );
        }
        return FailOrConvert(
            await repo.GetSessionsForProfile(profileID),
            sessions => sessions.Select(s => EFToViewConverter.Convert(s))
        );
    }
    
    //includes messages
    public async Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID) {
        StatusMessage sessionIDValidationStatus =
            await ValidateSessionID(monitoringSessionID);
        if(sessionIDValidationStatus.Failure()) {
            return DataActionResult<IEnumerable<MonitoringPulseResult>>.Failed(
                sessionIDValidationStatus
            );
        }
        return FailOrConvert(
            await repo.GetSessionReport(monitoringSessionID),
            pulses => pulses.Select(p => {
                IEnumerable<MonitoringMessage> messages = p.Messages
                    .Select(m => EFToViewConverter.Convert(m));
                MonitoringPulseResult pulse = EFToViewConverter.Convert(p);
                pulse.Messages = messages;
                return pulse;
            })
        );
    }
}

}