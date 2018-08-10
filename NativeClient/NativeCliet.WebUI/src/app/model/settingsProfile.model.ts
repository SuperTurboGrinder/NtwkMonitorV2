export class SettingsProfile {
    constructor(
        public id:number,
        public name:string,
        public sendMonitoringAlarm:boolean,
        public monitoringAlarmEmail:string,
        public monitoringStartHour:number, //from 0 to 23
        public monitoringEndHour:number,
        public startMonitoringOnLaunch:boolean,
        public depthMonitoring:boolean,
        public monitorInterval:number, //minutes
    ) {}
}