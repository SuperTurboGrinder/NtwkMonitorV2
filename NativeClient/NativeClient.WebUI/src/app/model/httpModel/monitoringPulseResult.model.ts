import { MonitoringMessage } from './monitoringMessage.model';

export class MonitoringPulseResult {
    public creationTimeJS: Date;

    public static convertJSTime(pulse: MonitoringPulseResult) {
        pulse.creationTimeJS = new Date(pulse.creationTime);
    }

    constructor(
        public responded: number,
        public silent: number,
        public skipped: number,
        public creationTime: number,
        public messages: MonitoringMessage[]
    ) {}
}
