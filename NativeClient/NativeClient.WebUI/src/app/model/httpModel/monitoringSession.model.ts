export class MonitoringSession {
    public creationTimeJS: Date;
    public lastPulseTimeJS: Date;

    constructor(
        public id: number,
        public createdByProfileId: number,
        public participatingNodesNum: number,
        public creationTime: number,
        public lastPulseTime: number
    ) {
    }

    public static convertJSTime(session: MonitoringSession) {
        session.creationTimeJS = new Date(session.creationTime);
        session.lastPulseTimeJS = new Date(session.lastPulseTime);
    }
}
