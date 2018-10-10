export class SettingsProfile {
    constructor(
        public id:number,
        public name:string,
        //public sendMonitoringAlarm:boolean,
        //public monitoringAlarmEmail:string,
        public monitoringStartHour:number, //from 0 to 23
        public monitoringSessionDuration:number, //(from 1 to 24) - startHour
        //public monitoringEndHour:number,
        public startMonitoringOnLaunch:boolean,
        public depthMonitoring:boolean,
        public monitorInterval:number, //minutes
    ) {}
}