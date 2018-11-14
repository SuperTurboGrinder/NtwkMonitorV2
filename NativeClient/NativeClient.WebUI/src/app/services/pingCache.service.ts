import { Injectable } from '@angular/core';
import { Subscription, BehaviorSubject, forkJoin } from 'rxjs';
import { first, skip } from 'rxjs/operators';

import { PingService } from './ping.service';
import { PingTestData } from '../model/httpModel/pingTestData.model';
import { SoundNotificatonService } from './soundNotificationService';

@Injectable()
export class PingCacheService {
    private pingCache = new Map<number, PingCacheCell>();

    constructor(
        private pingService: PingService,
        private sounds: SoundNotificatonService
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

    public treeUpdateWithoutCallback(roots: PingTree[]) {
        this.pingTree(
            roots,
            (success, _1, _2) => {
                if (success === false) {
                    this.sounds.playFailedPingAlarm();
                }
            },
            null
        );
    }

    public treeUpdateWithCallback(
        roots: PingTree[],
        forEachPingedCallback: (
            success: boolean,
            skipped: number,
            currentRoot: PingTree
        ) => void,
        finishedCallback: (result: TreePingFinishedData) => void,
        skipChildrenOfFailedNodes: boolean
    ) {
        this.pingTree(
            roots,
            (success, skipped, pingTreeNode) => {
                if (success === false) {
                    this.sounds.playFailedPingAlarm();
                }
                forEachPingedCallback(success, skipped, pingTreeNode);
            },
            finishedCallback,
            skipChildrenOfFailedNodes
        );
    }

    private countPingTreeNodes(pingTreeBranch: PingTree[]): number {
        let counter = 0;
        for (const p of pingTreeBranch) {
            counter += 1 + this.countPingTreeNodes(p.children);
        }
        return counter;
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

    private preparePingDataLayer(treeRoots: PingTree[]): {
        id: number,
        cell: PingCacheCell,
        pingTree: PingTree
    }[] {
        return treeRoots
            .filter(r => r.isPingable === true)
            .map(r => this.extractPingTreeRoot(r))
            .concat(
                treeRoots
                    .filter(r => r.isPingable === false)
                    .map(r => this.preparePingDataLayer(r.children))
                    .reduce((a, b) => a.concat(b), [])
            );
    }

    private pingTree(
        roots: PingTree[],
        forEachPingedCallback: (
            success: boolean,
            skipped: number,
            currentRoot: PingTree
        ) => void,
        finishedCallback: (result: TreePingFinishedData) => void,
        skipChildrenOfFailedNodes = true,
        tpfr: TreePingFinishedData = null
    ) {
        tpfr = (tpfr === null) ? new TreePingFinishedData(
            this.countPingTreeNodes(roots)
        ) : tpfr;
        const thisLayerPingDataList =
            this.preparePingDataLayer(roots);
        const pingObservables = thisLayerPingDataList.map(data => {
            data.cell.setLock();
            return this.pingService.getPing(data.id);
        });
        for (let i = 0; i < pingObservables.length; i++) {
            pingObservables[i].subscribe(pingResult => {
                const cell = thisLayerPingDataList[i].cell;
                const pingData = pingResult.data;
                cell.resetLock();
                cell.value.next(
                    pingResult.success === true
                        ? pingData
                        : null
                );
                const success = pingResult.success
                    && pingData.failed !== pingData.num;
                const children = thisLayerPingDataList[i].pingTree.children;
                const skipped = success === false
                && skipChildrenOfFailedNodes === true
                    ? this.countPingTreeNodes(
                        thisLayerPingDataList[i].pingTree.children
                    )
                    : 0;
                if (forEachPingedCallback !== null) {
                    forEachPingedCallback(
                        success,
                        skipped,
                        thisLayerPingDataList[i].pingTree
                    );
                }
                if (success) {
                    tpfr.successful++;
                } else {
                    tpfr.failed++;
                }
                tpfr.skipped += skipped;

                if (success === false
                && skipChildrenOfFailedNodes === true) {
                    this.setFailedBranch(children);
                } else {
                    this.pingTree(
                        children,
                        forEachPingedCallback,
                        finishedCallback,
                        skipChildrenOfFailedNodes,
                        tpfr
                    );
                }

                if (finishedCallback !== null
                && tpfr.successful
                + tpfr.failed
                + tpfr.skipped === tpfr.startNodesCount) {
                    finishedCallback(tpfr);
                }
           });
        }
    }

    /*
    private joinedPingTree(
        roots: PingTree[],
        forEachPingedCallback: (
            success: boolean,
            currentRoot: PingTree
        ) => void,
        skipChildrenOfFailedNodes = true
    ) {
        const thisLayerPingDataList =
            this.preparePingDataLayer(roots);
        forkJoin(
            thisLayerPingDataList.map(data => {
                data.cell.setLock();
                return this.pingService.getPing(data.id);
            })
        ).subscribe(pingResults => {
            for (let i = 0; i < pingResults.length; i++) {
                const cell = thisLayerPingDataList[i].cell;
                const pingData = pingResults[i].data;
                cell.resetLock();
                cell.value.next(
                    pingResults[i].success === true
                        ? pingData
                        : null
                );
                const success = pingResults[i].success
                    && pingData.failed !== pingData.num;
                const children = thisLayerPingDataList[i].pingTree.children;
                if (forEachPingedCallback !== null) {
                    forEachPingedCallback(
                        success,
                        thisLayerPingDataList[i].pingTree
                    );
                }

                if (success === false
                && skipChildrenOfFailedNodes === true) {
                    this.setFailedBranch(children);
                } else {
                    this.pingTree(
                        children, forEachPingedCallback
                    );
                }
            }
        });
    }
    */

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
    public name: string;
    public isPingable: boolean;
    public isBranchPingable: boolean;
    public children: PingTree[];
}

export class TreePingFinishedData {
    constructor(
        public startNodesCount: number,
        public successful: number = 0,
        public failed: number = 0,
        public skipped: number = 0
    ) {}
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
