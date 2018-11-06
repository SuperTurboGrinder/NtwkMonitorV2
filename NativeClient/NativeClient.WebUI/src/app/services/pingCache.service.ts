import { Injectable } from '@angular/core';
import { Subscription, BehaviorSubject, forkJoin } from 'rxjs';
import { first, skip } from 'rxjs/operators';

import { PingService } from './ping.service';
import { PingTestData } from '../model/httpModel/pingTestData.model';

@Injectable()
export class PingCacheService {
    private pingCache = new Map<number, PingCacheCell>();

    constructor(
        private pingService: PingService
    ) {}

    isLocked(nodeID: number): boolean {
        const cell = this.pingCache.get(nodeID);
        return cell === undefined
            ? false
            : cell.isLocked;
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
        if (cell.isLocked === false) {
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

    private pingListByID(
        nodesIDs: number[],
        finishCallback: () => void
    ) {
        const cellsAndIDs = nodesIDs.map(id => ({
            id: id,
            cell: this.getCell(id)
        })).filter(val => val.cell.isLocked === false);
        forkJoin(
            cellsAndIDs.map(cellAndId => {
                cellAndId.cell.setLock();
                return this.pingService.getPing(cellAndId.id);
            })
        ).subscribe(pingResults => {
            for (let i = 0; i < pingResults.length; i++) {
                const cell = cellsAndIDs[i].cell;
                cell.resetLock();
                cell.value.next(
                    pingResults[i].success === true
                        ? pingResults[i].data
                        : null
                );
            }
            finishCallback();
        });
    }

    public updateValues(
        nodesIDs: number[],
        finishCallback: () => void
    ) {
        this.pingListByID(nodesIDs, finishCallback);
    }

    public silentTreeUpdate(roots: PingTree[]) {
        this.pingTree(roots, null);
        // roots.forEach((tree) => {
        //    this.pingTree(tree, null);
        // });
    }

    private extractPingTreeRoot(root: PingTree): {
        id: number,
        cell: PingCacheCell,
        pingTree: PingTree
    } {
        return ({
            id: root.id,
            cell: this.getCell(root.id),
            pingTree: root
        });
    }

    private pingTree(
        roots: PingTree[],
        errorCallback: (currentRoot: PingTree) => void
    ) {
        const thisLayerPingDataList = roots.
            filter(r => r.isPingable === true)
            .map(r => this.extractPingTreeRoot(r))
            .concat(
                roots
                    .filter(r => r.isPingable === false)
                    .map(r => r.children
                        .map(c => this.extractPingTreeRoot(c))
                    )
                    .reduce((a, b) => a.concat(b))
            )
            .filter(val => val.cell.isLocked === false);
        const nonPingable
        console.log(currentPingDataList);
        forkJoin(
            currentPingDataList.map(data => {
                data.cell.setLock();
                return this.pingService.getPing(data.id);
            })
        ).subscribe(pingResults => {
            for (let i = 0; i < pingResults.length; i++) {
                const cell = currentPingDataList[i].cell;
                const pingData = pingResults[i].data;
                cell.resetLock();
                cell.value.next(
                    pingResults[i].success === true
                        ? pingData
                        : null
                );
                const success = pingResults[i].success
                    && pingData.failed !== pingData.num;
                const children = currentPingDataList[i].pingTree.children;
                if (success) {
                    this.pingTree(
                        children, errorCallback
                    );
                } else {
                    this.setFailedBranch(children);
                    if (errorCallback) {
                        errorCallback(currentPingDataList[i].pingTree);
                    }
                }
            }
        });
    }

    private pingTreeOld(
        root: PingTree,
        errorCallback: (currentRoot: PingTree) => void
    ) {
        if (root.isPingable) {
            this.subscribeToNextOnly(root.id,
                ptd => {
                    if (ptd.failed !== ptd.num) {
                        root.children.forEach(
                            nextRoot => this.pingTreeOld(nextRoot, errorCallback)
                        );
                    } else {
                        this.setFailedBranch(root.children);
                        if (errorCallback) {
                            errorCallback(root);
                        }
                    }
                }
            );
            this.pingByID(root.id);
        } else {
            root.children.forEach(
                nextRoot => this.pingTreeOld(nextRoot, errorCallback)
            );
        }
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

export class PingTree {
    public id: number;
    public isPingable: boolean;
    public isBranchPingable: boolean;
    public children: PingTree[];
}

class PingCacheCell {
    public value: BehaviorSubject<PingTestData>;
    private _isLocked = false;

    get isLocked(): boolean {
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
