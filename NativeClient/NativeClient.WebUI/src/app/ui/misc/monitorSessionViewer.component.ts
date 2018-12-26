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
    @Input() set resetFixed(v: {}) {
        if (this._resetFixed !== v) {
            this._resetFixed = v;
            this.fixedPulse = null;
        }
    }
    @Input() data: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    } =  ({ session: null, pulses: [] });
    @Input() isCurrentSession = false;
    @Input() currentlyPulsing = false;
    public messageTypes = MonitoringMessageType;
    private _selectedPulse: MonitoringPulseResult = null;
    private _resetFixed: {} = null;
    public fixedPulse: MonitoringPulseResult = null;

    public get session(): MonitoringSession {
        return this.data.session;
    }

    public get pulses(): MonitoringPulseResult[] {
        return this.data.pulses;
    }

    public get selectedPulse(): MonitoringPulseResult {
        const selected = this._selectedPulse !== null
            ? this._selectedPulse
            : this.fixedPulse !== null
                ? this.fixedPulse
                : MonitorSessionViewerComponent.emptyPulse;
        return selected;
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
}
