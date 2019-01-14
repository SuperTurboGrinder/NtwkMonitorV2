export class SettingsProfile {
    constructor(
        public id: number,
        public name: string,
        public startMonitoringOnLaunch: boolean,
        public depthMonitoring: boolean,
        public monitorInterval: number, // minutes
        public realTimePingUiUpdate
    ) {}
}
