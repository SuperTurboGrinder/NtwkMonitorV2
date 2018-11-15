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
    @Input() isCurrentSession = false;
    @Input() currentlyPulsing = false;
    private _selectedPulse: MonitoringPulseResult = null;
    public fixedPulse: MonitoringPulseResult = null;

    public get selectedPulse(): MonitoringPulseResult {
        return this._selectedPulse !== null
            ? this._selectedPulse
            : this.fixedPulse;
    }

    public get sessionStartTime(): string {
        return this.session.creationTimeJS.toLocaleTimeString();
    }

    public get sessionEndTime(): string {
        if (this.session.lastPulseTimeJS !== null) {
            return this.session.lastPulseTimeJS.toLocaleTimeString();
        } else {
            return '---';
        }
    }

    public get sessionDate(): string {
        return this.session.creationTimeJS.toLocaleDateString();
    }

    public get sessionNodeParticipants() {
        if (this.session !== null) {
            return this.session.participatingNodesNum;
        } else {
            return 0;
        }
    }

    public get selectedPulseCreationTime(): string {
        return this.selectedPulse.creationTimeJS.toLocaleTimeString();
    }

    public get selectedPulseMessages(): MonitoringMessage[] {
        return this.selectedPulse.messages;
    }

    public isMessageDanger(message: MonitoringMessage): boolean {
        return message.messageType !==
            MonitoringMessageType.Warning_InconsistentPing;
    }

    public formatMessage(message: MonitoringMessage): string {
        switch (message.messageType) {
            case MonitoringMessageType.Danger_NoPingReturned:
                return `No ping returned from ${message.messageSourceNodeName}`;
            case MonitoringMessageType.Danger_NoPingReturned_SkippedChildren:
                return `No ping returned from ${message.messageSourceNodeName}
                ; Skipped ${message.numSkippedChildren} child nodes`;
            case MonitoringMessageType.Warning_InconsistentPing:
                return `Some ping packets lost to ${message.messageSourceNodeName}`;
        }
    }

    public selectPulse(i: number) {
        this._selectedPulse = this.pulses[i];
    }

    public fixPulse(i: number) {
        this.fixedPulse = this.pulses[i];
    }

    public deselectPulse(_: MouseEvent) {
        this._selectedPulse = null;
    }

    pulseTrackByFn(index: number, pulse: MonitoringPulseResult) {
        return pulse.creationTime;
    }

    constructor() {
        this.testValuesGeneration();
    }

    testValuesGeneration() {
        const sessionStart = Math.random() * 90000000000;
        const sessionEnd = sessionStart + Math.random() * 9000000;
        this.session =
            new MonitoringSession(0, 0, 38, sessionStart, sessionEnd);
        this.session.convertJSTime();

        const pingedNodes = 38;
        const result: MonitoringPulseResult[] = [];
        for (let i = 0; i < 120; i++) {
            const success = Math.random() > 0.5;
            if (success) {
                for (let j = 0; j < 10; j++) {
                    const pulse = new MonitoringPulseResult(
                        pingedNodes, 0, 0, Math.random() * 90000000000, []
                    );
                    pulse.convertJSTime();
                    result.push(pulse);
                }
            } else {
                const failedNodes = 2;
                const skippedNodes = Math.floor((pingedNodes - 2) * Math.random());
                for (let j = 0; j < 10; j++) {
                    const pulse = new MonitoringPulseResult(
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
                    );
                    pulse.convertJSTime();
                    result.push(pulse);
                }
            }
        }
        this.pulses = result;
    }

}
