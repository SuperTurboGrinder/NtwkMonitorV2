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

namespace Data.DataServices.Services
{
    public class MonitoringDataService
        : BaseDataService, IMonitoringDataService
    {
        readonly IViewModelValidator _validator;
        readonly IViewModelToEfModelConverter _viewToEfConverter;
        readonly IEfModelToViewModelConverter _efToViewConverter;

        public MonitoringDataService(
            IDataRepository repo,
            IViewModelValidator validator,
            IViewModelToEfModelConverter viewToEfConverter,
            IEfModelToViewModelConverter efToViewConverter
        ) : base(repo)
        {
            _validator = validator;
            _viewToEfConverter = viewToEfConverter;
            _efToViewConverter = efToViewConverter;
        }

        public async Task<DataActionResult<MonitoringSession>> GetNewSession(
            int profileId
        )
        {
            StatusMessage profIdValidationStatus =
                await ValidateProfileId(profileId);
            if (profIdValidationStatus.Failure())
            {
                return DataActionResult<MonitoringSession>.Failed(profIdValidationStatus);
            }

            return FailOrConvert(
                await Repo.GetNewSession(profileId),
                session => _efToViewConverter.Convert(session)
            );
        }

        public async Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
            int sessionId,
            MonitoringPulseResult pulseResult,
            IEnumerable<MonitoringMessage> messages
        )
        {
            StatusMessage sessionIdValidationStatus =
                await ValidateSessionId(sessionId);
            if (sessionIdValidationStatus.Failure())
            {
                return DataActionResult<MonitoringPulseResult>.Failed(sessionIdValidationStatus);
            }

            IEnumerable<MonitoringMessage> monitoringMessages = messages as MonitoringMessage[] ?? messages.ToArray();
            foreach (MonitoringMessage message in monitoringMessages)
            {
                StatusMessage messageValidationStatus = _validator.Validate(message);
                if (messageValidationStatus.Failure())
                {
                    return DataActionResult<MonitoringPulseResult>.Failed(
                        messageValidationStatus
                    );
                }
            }

            EFDbModel.MonitoringPulseResult convertedPulseResult =
                _viewToEfConverter.Convert(pulseResult);
            IEnumerable<EFDbModel.MonitoringMessage> convertedMessages = monitoringMessages
                .Select(m => _viewToEfConverter.Convert(m));
            return FailOrConvert(
                await Repo.SavePulseResult(sessionId, convertedPulseResult, convertedMessages),
                ConvertPulseResult
            );
        }

        private MonitoringPulseResult ConvertPulseResult(
            EFDbModel.MonitoringPulseResult pulseResult
        )
        {
            IEnumerable<MonitoringMessage> messages = pulseResult.Messages
                .Select(m => _efToViewConverter.Convert(m));
            MonitoringPulseResult pulse = _efToViewConverter.Convert(pulseResult);
            pulse.Messages = messages;
            return pulse;
        }

        public async Task<StatusMessage> ClearEmptySessions()
        {
            return await Repo.ClearEmptySessions();
        }

        public async Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileId)
        {
            StatusMessage profIdValidationStatus =
                await ValidateProfileId(profileId);
            if (profIdValidationStatus.Failure())
            {
                return DataActionResult<IEnumerable<MonitoringSession>>.Failed(
                    profIdValidationStatus
                );
            }

            return FailOrConvert(
                await Repo.GetSessionsForProfile(profileId),
                sessions => sessions.Select(s => _efToViewConverter.Convert(s))
            );
        }

        //includes messages
        public async Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(
            int monitoringSessionId)
        {
            StatusMessage sessionIdValidationStatus =
                await ValidateSessionId(monitoringSessionId);
            if (sessionIdValidationStatus.Failure())
            {
                return DataActionResult<IEnumerable<MonitoringPulseResult>>.Failed(
                    sessionIdValidationStatus
                );
            }

            return FailOrConvert(
                await Repo.GetSessionReport(monitoringSessionId),
                pulses => pulses.Select(ConvertPulseResult)
            );
        }
    }
}