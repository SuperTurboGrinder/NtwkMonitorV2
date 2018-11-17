import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { MonitoringSession } from 'src/app/model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from 'src/app/model/httpModel/monitoringPulseResult.model';
import { MonitoringMessage } from 'src/app/model/httpModel/monitoringMessage.model';
import { MonitoringMessageType } from 'src/app/model/httpModel/monitoringMessageType.model';

@Component({
    selector: 'app-monitor-session-viewer',
    templateUrl: './monitorSessionViewer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MonitorSessionViewerComponent {
    private static testSession = null;
    private static testPulses = null;
    private static readonly emptyPulse = (() => {
        const pulse = new MonitoringPulseResult(
            0, 0, 0, 0, []
        );
        MonitoringPulseResult.convertJSTime(pulse);
        return pulse;
    })();
    @Input() data: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    } = { session: null, pulses: [] };
    @Input() isCurrentSession = false;
    @Input() currentlyPulsing = false;
    private _selectedPulse: MonitoringPulseResult = null;
    public fixedPulse: MonitoringPulseResult = null;

    public get cdTest(): string {
        const time = new Date().toLocaleTimeString();
        console.log(`Change check (${time})`);
        return time;
    }

    public get session(): MonitoringSession {
        return this.data.session;
    }

    public get pulses(): MonitoringPulseResult[] {
        return this.data.pulses;
    }

    public get selectedPulse(): MonitoringPulseResult {
        return this._selectedPulse !== null
            ? this._selectedPulse
            : this.fixedPulse !== null
                ? this.fixedPulse
                : MonitorSessionViewerComponent.emptyPulse;
    }

    public get isSelected(): boolean {
        return this.fixedPulse !== null || this._selectedPulse !== null;
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

    public selectPulse(pulse: MonitoringPulseResult) {
        this._selectedPulse = pulse;
    }

    public fixPulse(pulse: MonitoringPulseResult) {
        this.fixedPulse = pulse;
    }

    public deselectPulse(_: MouseEvent) {
        this._selectedPulse = null;
    }

    pulseTrackByFn(index: number, pulse: MonitoringPulseResult) {
        return pulse.creationTime;
    }

    constructor() {
        // this.testValuesGeneration();
    }

    testValuesGeneration() {
        if (MonitorSessionViewerComponent.testSession === null) {
            const sessionStart = Math.random() * 90000000000;
            const sessionEnd = sessionStart + Math.random() * 9000000;
            MonitorSessionViewerComponent.testSession =
                new MonitoringSession(0, 0, 38, sessionStart, sessionEnd);
            MonitorSessionViewerComponent.testSession.convertJSTime();

            const pingedNodes = 38;
            const result: MonitoringPulseResult[] = [];
            for (let i = 0; i < 120; i++) {
                const success = Math.random() > 0.5;
                if (success) {
                    for (let j = 0; j < 10; j++) {
                        const pulse = new MonitoringPulseResult(
                            pingedNodes, 0, 0, Math.random() * 90000000000, []
                        );
                        MonitoringPulseResult.convertJSTime(pulse);
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
                        MonitoringPulseResult.convertJSTime(pulse);
                        result.push(pulse);
                    }
                }
            }
            MonitorSessionViewerComponent.testPulses = result;
        }
        this.data = {
            session: MonitorSessionViewerComponent.testSession,
            pulses: MonitorSessionViewerComponent.testPulses
        };
    }

}
