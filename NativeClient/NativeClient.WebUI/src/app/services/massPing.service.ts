import { Injectable } from '@angular/core';

import { PingTestData } from '../model/httpModel/pingTestData.model';
import { SoundNotificatonService } from './soundNotificationService';
import { PingCacheService } from './pingCache.service';
import { BehaviorSubject, Subscription } from 'rxjs';

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

export class PingProgress {
    constructor(
        public current: number,
        public end: number
    ) {}
}

@Injectable()
export class MassPingService {
    private pingProgressSubject = new BehaviorSubject<PingProgress>(null);
    private isInProgress = false;

    constructor(
        private pingCacheService: PingCacheService,
        private sounds: SoundNotificatonService
    ) {}

    public get inProgress(): boolean {
        return this.isInProgress;
    }

    public subscribeToPingProgress(
        callback: (PingProgress) => void
    ): Subscription {
        return this.pingProgressSubject.asObservable().subscribe(callback);
    }

    public pingRange(
        nodesIDs: number[],
        finishCallback: () => void
    ) {
        if (this.setInProgress(-1)) {
            this.pingCacheService.pingListByID(
                nodesIDs,
                true,
                (_) => {
                    this.resetInProgress();
                    finishCallback();
                });
        }
    }

    public pingTreeWithoutCallback(
        roots: PingTree[],
    ) {
        this.pingTree(
            roots,
            (success, _1, _2) => {
                if (success === false) {
                    this.sounds.playFailedPingAlarm();
                }
            },
            _ => this.resetInProgress()
        );
    }

    public pingTreeWithCallback(
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
            result => {
                this.resetInProgress();
                finishedCallback(result);
            },
            skipChildrenOfFailedNodes
        );
    }

    private setInProgress(numParticipants: number): boolean {
        if (this.isInProgress === true) {
            return false;
        }
        this.isInProgress = true;
        this.pingProgressSubject.next(
            new PingProgress(0, numParticipants)
        );
        return true;
    }

    private resetInProgress() {
        this.isInProgress = false;
        this.pingProgressSubject.next(null);
    }

    private nextProgress(numDone: number) {
        const current = this.pingProgressSubject.value;
        if (current !== null) {
            this.pingProgressSubject.next(
                new PingProgress(current.current + numDone, current.end)
            );
        }
    }

    private countPingablePingTreeNodes(pingTreeBranch: PingTree[]): number {
        let counter = 0;
        for (const p of pingTreeBranch) {
            if (p !== null) {
                counter += (p.isPingable ? 1 : 0)
                    + this.countPingablePingTreeNodes(p.children);
            }
        }
        return counter;
    }

    private preparePingDataLayer(treeRoots: PingTree[]): PingTree[] {
        return treeRoots
            .filter(r => r.isPingable === true)
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
        if (tpfr === null) {
            const count = this.countPingablePingTreeNodes(roots);
            if (this.setInProgress(count)) {
                tpfr = new TreePingFinishedData(count);
            } else {
                return;
            }
        }
        const thisLayerPingDataList =
            this.preparePingDataLayer(roots);
        const thisLayerLockedNodesIDs = thisLayerPingDataList.map(pd => {
            return pd.id;
        });
        let nextLayer: PingTree[] = [];
        this.pingCacheService.pingListByID(
            thisLayerLockedNodesIDs,
            true,
            (data: PingTestData[]) => {
                const allFailed = data === null;
                if (allFailed) {
                    let skippedSumm = 0;
                    for (let i = 0; i < thisLayerPingDataList.length; i++) {
                        const children = thisLayerPingDataList[i].children;
                        const skipped = skipChildrenOfFailedNodes === true
                            ? this.countPingablePingTreeNodes(children)
                            : 0;
                        tpfr.failed++;
                        skippedSumm += skipped;
                        tpfr.skipped += skipped;
                        if (skipChildrenOfFailedNodes === true) {
                            this.pingCacheService.setFailedBranch(children);
                        }
                        if (forEachPingedCallback !== null) {
                            forEachPingedCallback(
                                false,
                                skipped,
                                thisLayerPingDataList[i]
                            );
                        }
                    }
                    this.nextProgress(skippedSumm);
                } else {
                    for (let i = 0; i < thisLayerPingDataList.length; i++) {
                        const pingData = data[i];
                        const success = pingData.failed !== pingData.num;
                        const children = thisLayerPingDataList[i].children;
                        let skipped = 0;
                        if (success) {
                            tpfr.successful++;
                            nextLayer = nextLayer.concat(children);
                        } else {
                            tpfr.failed++;
                            if (skipChildrenOfFailedNodes === true) {
                                skipped = this.countPingablePingTreeNodes(children);
                                tpfr.skipped += skipped;
                                this.nextProgress(skipped);
                                this.pingCacheService.setFailedBranch(children);
                            }
                        }
                        if (forEachPingedCallback !== null) {
                            forEachPingedCallback(
                                success,
                                skipped,
                                thisLayerPingDataList[i]
                            );
                        }
                    }
                }
                this.nextProgress(thisLayerLockedNodesIDs.length);
                if (nextLayer.length > 0) {
                    this.pingTree(
                        nextLayer,
                        forEachPingedCallback,
                        finishedCallback,
                        skipChildrenOfFailedNodes,
                        tpfr
                    );
                }

                const finished = tpfr.successful + tpfr.failed + tpfr.skipped === tpfr.startNodesCount;
                if (finished === true) {
                    this.resetInProgress();
                    if (finishedCallback !== null) {
                        finishedCallback(tpfr);
                    }
                }
            }
        );
    }
}
