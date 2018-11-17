import { Component, Output, EventEmitter, OnDestroy, Input, ChangeDetectionStrategy } from '@angular/core';
import { PingCacheService } from '../../services/pingCache.service';
import { Subscription } from 'rxjs';
import { NtwkNode } from '../../model/httpModel/ntwkNode.model';
import { PingTestData } from '../../model/httpModel/pingTestData.model';

@Component({
    selector: 'app-ping-display',
    templateUrl: './pingDisplay.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PingDisplayComponent implements OnDestroy {
    private _node: NtwkNode;
    private pingValue: PingTestData = null;
    private subsctiptions: Subscription[] = [];
    public isTryingToPing = false;
    private cacheIsLocked = true;

    @Output() changedEvent = new EventEmitter<boolean>();

    @Input()
    set node(node: NtwkNode) {
        this._node = node;
        if (this.subsctiptions.length !== 0) {
            this.unsubscribeFromPingCacheCellData();
            this.pingValue = null;
        }
        const valueSubsctiption = this.pingCache.subscribeToValue(
            this.node.id,
            (ptd) => {
                this.isTryingToPing = false;

                const wasChanged = this.pingValue === null
                    || (this.pingValue.avg !== ptd.avg
                    || this.pingValue.num !== ptd.num
                    || this.pingValue.failed !== ptd.failed);
                this.changedEvent.emit(wasChanged);
                this.pingValue = ptd;
            }
        );
        const lockSubscription = this.pingCache.lockStatusObservable(this.node.id)
            .subscribe(lockStatus => {
                this.cacheIsLocked = lockStatus;
                this.changedEvent.emit(false);
            });
        this.subsctiptions = [valueSubsctiption, lockSubscription];
    }

    get node() {
        return this._node;
    }

    constructor(
        private pingCache: PingCacheService
    ) {}

    private unsubscribeFromPingCacheCellData() {
        this.subsctiptions.forEach(sub => sub.unsubscribe());
    }

    public pingAverage(): string {
        return this.pingValue === null
            ? '--'
            : this.pingValue.avg.toString();
    }

    public get pingOK(): boolean {
        return this.pingValue != null
            && this.pingValue.avg < 50;
    }

    public get pingWarning(): boolean {
        return this.pingValue != null
            && this.pingValue.avg >= 50;
    }

    public get pingDanger(): boolean {
        return this.pingValue != null
            && this.pingValue.num === this.pingValue.failed;
    }

    public get pingCountWarning() {
        return this.pingValue != null
            && this.pingValue.failed !== 0
            && this.pingValue.failed !== this.pingValue.num;
    }

    public get pingCountDanger() {
        return this.pingValue != null
            && this.pingValue.failed !== 0
            && this.pingValue.failed === this.pingValue.num;
    }

    public pingCount(): string {
        return this.pingValue === null
            ? '-'
            : this.pingValue.num.toString();
    }

    public pingSuccessful(): string {
        return this.pingValue === null
            ? '-'
            : (this.pingValue.num - this.pingValue.failed).toString();
    }

    public pingNumber(): string {
        return this.pingValue === null
            ? '-'
            : this.pingValue.num.toString();
    }

    tryToPing() {
        if (!this.isTryingToPing) {
            this.isTryingToPing = true;
            this.pingCache.updateValue(this.node.id);
        }
    }

    get isButtonDisabled() {
        return this.node.isOpenPing === false
            || this.isTryingToPing
            || this.cacheIsLocked;
    }

    ngOnDestroy() {
        this.unsubscribeFromPingCacheCellData();
    }
}
