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

    public getIDsSortedByPing(descending: boolean) : number[] {
        let lessWithDescending = () => descending ? 1 : -1;
        let bothAreFailed = (val1, val2) =>
            val1.ptd.num == val1.ptd.failed && val2.ptd.num == val2.ptd.failed;
        let onlyFirstIsFailed = (val1, val2) =>
            val1.ptd.num == val1.ptd.failed && val2.ptd.num != val2.ptd.failed;
        let compare = (val1, val2) =>
            val1.ptd.avg == val2.ptd.avg
                ? 0 : val1.ptd.avg < val2.ptd.avg
                    ? lessWithDescending() : -lessWithDescending();
        let someAreFailedOr = (a: {id:number, ptd: PingTestData}, b: {id:number, ptd: PingTestData},
            or: (v1, v2) => number
        ): number => bothAreFailed(a, b)
                ? 0 : onlyFirstIsFailed(a, b)
                    ? -1 : onlyFirstIsFailed(b, a)
                        ? 1 : or(a, b);
        return Array.from(this.pingCache.entries())
            .map(val => {
                return {id: val[0], ptd: val[1].value.getValue()};
            })
            .filter(val => val.ptd !== null)
            .sort((a, b) => someAreFailedOr(a, b, compare))
            .map(val => val.id);
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