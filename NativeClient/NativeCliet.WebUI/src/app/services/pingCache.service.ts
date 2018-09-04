import { Injectable } from "@angular/core";
import { Subscription, BehaviorSubject } from "rxjs"

import { PingService } from "./ping.service";
import { PingTestData } from "../model/httpModel/pingTestData.model";

@Injectable()
export class PingCacheService {
    private pingCache = new Map<number, PingCacheCell>();

    constructor(
        private pingService: PingService
    ) {}

    isLocked(nodeID: number) : boolean {
        let cell = this.pingCache.get(nodeID);
        return cell == undefined
            ? false
            : cell.isLocked;
    }

    private getCell(nodeID: number) : PingCacheCell {
        let cell = this.pingCache.get(nodeID);
        if(cell == undefined) {
            cell = new PingCacheCell(null);
            this.pingCache.set(nodeID, cell);
        }
        return cell;
    }

    public subscribeToValue(
        nodeID: number,
        callback: (ptd:PingTestData)=>void
    ) : Subscription {
        return this.getCell(nodeID).value.subscribe(callback);
    }

    public updateValue(nodeID: number) {
        let cell = this.getCell(nodeID);
        if(cell.isLocked == false) {
            cell.setLock();
            this.pingService.getPing(nodeID).subscribe(pingTestResult => {
                cell.resetLock()
                cell.value.next(
                    pingTestResult.success === true
                        ? pingTestResult.data
                        : null
                )
            });
        }

    }
}

class PingCacheCell {
    public value: BehaviorSubject<PingTestData>;
    private _isLocked: boolean = false;

    get isLocked() : boolean {
        return this._isLocked;
    }

    setLock() {
        this._isLocked = true;
    }

    resetLock() {
        this._isLocked = false;
    }

    constructor(
        initialValue: PingTestData
    ) {
        this.value = new BehaviorSubject<PingTestData>(initialValue);
    }
}