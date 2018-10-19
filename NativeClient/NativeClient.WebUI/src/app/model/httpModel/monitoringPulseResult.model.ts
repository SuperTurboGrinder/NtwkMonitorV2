import { MonitoringMessage } from './monitoringMessage.model';

export class MonitoringPulseResult {
    public creationTimeJS: Date;

    constructor(
        public responded: number,
        public silent: number,
        public skipped: number,
        public creationTime: number,
        public messages: MonitoringMessage[]
    ) {}

    convertJSTime() {
        this.creationTimeJS = new Date(this.creationTime);
    }
}
