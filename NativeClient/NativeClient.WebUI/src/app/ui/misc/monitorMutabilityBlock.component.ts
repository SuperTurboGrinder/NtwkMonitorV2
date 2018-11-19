import { Component, Input, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { MonitorBlockerService } from 'src/app/services/monitorBlocker.service';
import { PingMonitorService } from 'src/app/services/pingMonitor.service';

@Component({
    selector: 'app-monitor-mutability-block',
    templateUrl: './monitorMutabilityBlock.component.html'
})
export class MonitorMutabilityBlockComponent implements OnDestroy {
    constructor(
        private pingMonitor: PingMonitorService,
        private monitorBlocker: MonitorBlockerService
    ) {
        monitorBlocker.hideMonitor();
    }

    public get shouldBlock(): boolean {
        return this.pingMonitor.isActive;
    }

    ngOnDestroy() {
        this.monitorBlocker.showMonitor();
    }
}
