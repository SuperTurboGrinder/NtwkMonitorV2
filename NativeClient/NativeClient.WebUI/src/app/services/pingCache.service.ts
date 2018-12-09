import { Injectable } from '@angular/core';
import { Subscription, BehaviorSubject, forkJoin, Observable, of } from 'rxjs';
import { first, skip } from 'rxjs/operators';

import { PingService } from './ping.service';
import { PingTestData } from '../model/httpModel/pingTestData.model';
import { SoundNotificatonService } from './soundNotificationService';
import { PingTree } from './massPing.service';

@Injectable()
export class PingCacheService {
    private pingCache = new Map<number, PingCacheCell>();
    private massPingInProgress = false;

    constructor(
        private pingService: PingService,
        private sounds: SoundNotificatonService
    ) {}

    public lockStatusObservable(nodeID: number): Observable<boolean> {
        const cell = this.pingCache.get(nodeID);
        return cell === undefined
            ? of(false)
            : cell.lockedStatusObservable;
    }

    private getCell(nodeID: number): PingCacheCell {
        let cell = this.pingCache.get(nodeID);
        if (cell === undefined) {
            cell = new PingCacheCell(null);
            this.pingCache.set(nodeID, cell);
        }
        return cell;
    }

    public getIDsSortedByPing(descending: boolean): number[] {
        const lessWithDescending = () => descending ? 1 : -1;
        const bothAreFailed = (val1, val2) =>
            val1.ptd.num === val1.ptd.failed && val2.ptd.num === val2.ptd.failed;
        const onlyFirstIsFailed = (val1, val2) =>
            val1.ptd.num === val1.ptd.failed && val2.ptd.num !== val2.ptd.failed;
        const compare = (val1, val2) =>
            val1.ptd.avg === val2.ptd.avg
                ? 0 : val1.ptd.avg < val2.ptd.avg
                    ? lessWithDescending() : -lessWithDescending();
        const someAreFailedOr = (a: {id: number, ptd: PingTestData}, b: {id: number, ptd: PingTestData},
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
        callback: (ptd: PingTestData) => void
    ): Subscription {
        return this.getCell(nodeID).value.subscribe(callback);
    }

    private subscribeToNextOnly(
        nodeID: number,
        callback: (ptd: PingTestData) => void
    ) {
        this.getCell(nodeID).value.pipe(skip(1), first()).subscribe(callback);
    }

    private pingByID(nodeID: number) {
        const cell = this.getCell(nodeID);
        if (cell.currentLockStatus === false) {
            cell.setLock();
            this.pingService.getPing(nodeID).subscribe(pingTestResult => {
                cell.resetLock();
                cell.value.next(
                    pingTestResult.success === true
                        ? pingTestResult.data
                        : null
                );
            });
        }
    }

    public updateValue(nodeID: number) {
        this.pingByID(nodeID);
    }

    public pingListByID(
        nodesIDs: number[],
        lock: boolean,
        finishCallback: (data: PingTestData[]) => void
    ) {
        const cellsAndIDs = nodesIDs.map(id => ({
            id: id,
            cell: this.getCell(id)
        })).filter(val => val.cell.currentLockStatus === false);
        const idsToPing = cellsAndIDs.map(val => {
            if (lock === true) { val.cell.setLock(); }
            return val.id;
        });
        this.pingService.getListPing(idsToPing)
        .subscribe(pingResult => {
            const pingFailed = pingResult.success === false;
            for (let i = 0; i < cellsAndIDs.length; i++) {
                const cell = cellsAndIDs[i].cell;
                cell.resetLock();
                cell.value.next(
                    pingFailed
                        ? null
                        : pingResult.data[i]
                );
            }
            finishCallback(pingResult.data);
        });
    }

    setFailedBranch(roots: PingTree[]) {
        roots.forEach(root => {
            const cell = this.getCell(root.id);
            cell.value.next({
                failed: 0,
                num: 0,
                avg: 0
            });
            this.setFailedBranch(root.children);
        });
    }
}

class PingCacheCell {
    public value: BehaviorSubject<PingTestData>;
    private lockStatus = new BehaviorSubject<boolean>(false);

    public get currentLockStatus(): boolean {
        return this.lockStatus.value;
    }

    public get lockedStatusObservable(): Observable<boolean> {
        return this.lockStatus.asObservable();
    }

    public setLock() {
        this.lockStatus.next(true);
    }

    public resetLock() {
        this.lockStatus.next(false);
    }

    constructor(
        initialValue: PingTestData
    ) {
        this.value = new BehaviorSubject<PingTestData>(initialValue);
    }
}
