import { Component } from '@angular/core';
import { PingMonitorService } from '../services/pingMonitor.service';

@Component({
    selector: 'app-ping-monitor-panel',
    templateUrl: './pingMonitorPanel.component.html'
})
export class PingMonitorPanelComponent {
    public isOpened = false;

    constructor(
        private pingMonitorService: PingMonitorService
    ) { }

    public get isActive() {
        return this.pingMonitorService.isActive;
    }
}
