export class MonitoringSession {
    public creationTimeJS: Date;
    public lastPulseTimeJS: Date;

    constructor(
        public id: number,
        public createdByProfileID: number,
        public participatingNodesNum: number,
        public creationTime: number,
        public lastPulseTime: number
    ) {
    }

    convertJSTime() {
        this.creationTimeJS = new Date(this.creationTime);
        this.lastPulseTimeJS = new Date(this.lastPulseTime);
    }
}
