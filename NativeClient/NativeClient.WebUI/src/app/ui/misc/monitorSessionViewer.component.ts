import { Component, Input } from '@angular/core';
import { MonitoringSession } from 'src/app/model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from 'src/app/model/httpModel/monitoringPulseResult.model';
import { MonitoringMessage } from 'src/app/model/httpModel/monitoringMessage.model';
import { MonitoringMessageType } from 'src/app/model/httpModel/monitoringMessageType.model';

@Component({
    selector: 'app-monitor-session-viewer',
    templateUrl: './monitorSessionViewer.component.html'
})
export class MonitorSessionViewerComponent {
    @Input() session: MonitoringSession = null;
    @Input() pulses: MonitoringPulseResult[] = [];
    @Input() currentlyPulsing = false;

    private componentCreatinDate = new Date();

    public get sessionStartTime(): Date {
        if (this.session !== null) {
            return this.session.creationTimeJS;
        } else {
            return this.componentCreatinDate;
        }
    }

    public get sessionNodeParticipants() {
        if (this.session !== null) {
            return this.session.participatingNodesNum;
        } else {
            return 0;
        }
    }

    constructor() {
        this.testValuesGeneration();
    }

    testValuesGeneration() {
        const pingedNodes = 38;
        const result: MonitoringPulseResult[] = [];
        for (let i = 0; i < 120; i++) {
            const success = Math.random() > 0.5;
            if (success) {
                for (let j = 0; j < 10; j++) {
                    result.push(new MonitoringPulseResult(
                        pingedNodes, 0, 0, 0, []
                    ));
                }
            } else {
                const failedNodes = 2;
                const skippedNodes = (pingedNodes - 2) * Math.random();
                for (let j = 0; j < 10; j++) {
                    result.push(new MonitoringPulseResult(
                        pingedNodes - skippedNodes - failedNodes,
                        failedNodes,
                        skippedNodes,
                        0,
                        [
                            new MonitoringMessage(
                                MonitoringMessageType.Danger_NoPingReturned,
                                'nodeNameTest1',
                                0
                            ),
                            new MonitoringMessage(
                                MonitoringMessageType.Danger_NoPingReturned_SkippedChildren,
                                'nodeNameTest2',
                                skippedNodes
                            )
                        ]
                    ));
                }
            }
        }
        this.pulses = result;
    }

}
