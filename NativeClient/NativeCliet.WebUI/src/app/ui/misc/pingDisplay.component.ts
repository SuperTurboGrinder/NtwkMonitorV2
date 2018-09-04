import { Component, OnDestroy, Input } from "@angular/core";
import { PingCacheService } from "../../services/pingCache.service";
import { Subscription } from "rxjs";
import { NtwkNode } from "../../model/httpModel/ntwkNode.model";
import { PingTestData } from "../../model/httpModel/pingTestData.model"

@Component({
    selector: 'pingDisplay',
    templateUrl: './pingDisplay.component.html'
})
export class PingDisplayComponent implements OnDestroy {
    private _node: NtwkNode;
    private pingValue: PingTestData = null;
    private subsctiption: Subscription = null;
    public isTryingToPing: boolean = false;

    @Input()
    set node(node: NtwkNode) {
        this._node = node;
        if(this.subsctiption != null) {
            this.subsctiption.unsubscribe();
        }
        this.subsctiption = this.pingCache.subscribeToValue(
            this.node.id,
            (ptd) => {
                this.isTryingToPing = false;
                this.pingValue = ptd
            }
        );
    }

    get node() {
        return this._node;
    }

    constructor(
        private pingCache: PingCacheService
    ) {}

    public pingAverage() : string {
        return this.pingValue === null
            ? "--"
            : this.pingValue.avg.toString();
    }

    public get pingOK() : boolean {
        return this.pingValue != null
            && this.pingValue.avg < 50;
    }

    public get pingWarning() : boolean {
        return this.pingValue != null
            && this.pingValue.avg >= 50;
    }

    public get pingDanger() : boolean {
        return this.pingValue != null
            && this.pingValue.num == this.pingValue.failed;
    }

    public get pingCountWarning() {
        return this.pingValue != null
            && this.pingValue.failed != 0
            && this.pingValue.failed != this.pingValue.num;
    }
    
    public get pingCountDanger() {
        return this.pingValue != null
            && this.pingValue.failed != 0
            && this.pingValue.failed == this.pingValue.num;
    }

    public pingCount() : string {
        return this.pingValue === null
            ? "-"
            : this.pingValue.num.toString();
    }

    public pingSuccessful() : string {
        return this.pingValue === null
            ? "-"
            : (this.pingValue.num-this.pingValue.failed).toString();
    }

    public pingNumber() : string {
        return this.pingValue === null
            ? "-"
            : this.pingValue.num.toString();
    }

    tryToPing() {
        if(!this.isTryingToPing) {
            this.isTryingToPing = true;
            this.pingCache.updateValue(this.node.id);
        }
    }

    get isButtonDisabled() {
        return this.node.isOpenPing == false
            || this.isTryingToPing
            || this.pingCache.isLocked(this.node.id);
    }

    ngOnDestroy() {
        this.subsctiption.unsubscribe();
    }
}