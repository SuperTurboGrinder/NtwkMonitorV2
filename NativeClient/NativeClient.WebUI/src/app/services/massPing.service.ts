import { Injectable } from '@angular/core';
import { Subscription, BehaviorSubject, forkJoin, Observable, of } from 'rxjs';
import { first, skip } from 'rxjs/operators';

import { PingService } from './ping.service';
import { PingTestData } from '../model/httpModel/pingTestData.model';
import { SoundNotificatonService } from './soundNotificationService';
import { PingCacheService } from './pingCache.service';

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

@Injectable()
export class MassPingService {
    constructor(
        private pingCacheService: PingCacheService,
        private sounds: SoundNotificatonService
    ) {}

    public pingRange(
        nodesIDs: number[],
        finishCallback: () => void
    ) {
        this.pingCacheService.pingListByID(nodesIDs, null, (_) => finishCallback());
    }

    public pingTreeWithoutCallback(
        roots: PingTree[],
        count: (count: number) => void
    ) {
        this.pingTree(
            roots,
            count,
            (success, _1, _2) => {
                if (success === false) {
                    this.sounds.playFailedPingAlarm();
                }
            },
            null
        );
    }

    public pingTreeWithCallback(
        roots: PingTree[],
        count: (count: number) => void,
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
            count,
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

    private countPingablePingTreeNodes(pingTreeBranch: PingTree[]): number {
        let counter = 0;
        for (const p of pingTreeBranch) {
            if (p !== null) {
                counter += p.isPingable ? 1 : 0
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
        startedCallback: (allCount: number) => void,
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
            tpfr = new TreePingFinishedData(
                this.countPingablePingTreeNodes(roots)
            );
            if (startedCallback !== null) { startedCallback(tpfr.startNodesCount); }
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
                if (allFailed) { return; }
                for (let i = 0; i < thisLayerPingDataList.length; i++) {
                    const pingData = data[i];
                    let success = allFailed === false;
                    let skipped = 0;
                    const children = thisLayerPingDataList[i].children;
                    if (success) {
                        success = pingData.failed !== pingData.num;
                    } else {
                        skipped = skipChildrenOfFailedNodes === true
                            ? this.countPingablePingTreeNodes(children)
                            : 0;
                    }
                    if (forEachPingedCallback !== null) {
                        forEachPingedCallback(
                            success,
                            skipped,
                            thisLayerPingDataList[i]
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
                        this.pingCacheService.setFailedBranch(children);
                    } else {
                        nextLayer = nextLayer.concat(children);
                    }
                }
                if (nextLayer.length > 0) {
                    this.pingTree(
                        nextLayer,
                        startedCallback,
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
            }
        );
    }
}
