export class MonitoringMessage {
    constructor(
        public messageType: string,
        public messageSourceNodeName: string,
        public numSkippedChildren: string
    ) {}
}
