import { Injectable, OnDestroy } from '@angular/core';

import { PingTestData } from '../model/httpModel/pingTestData.model';
import { SoundNotificatonService } from './soundNotificationService';
import { PingCacheService } from './pingCache.service';
import { BehaviorSubject, Subscription } from 'rxjs';
import { SettingsProfilesService } from './settingsProfiles.service';
import { MassPingCancellator } from '../model/servicesModel/massPingCancelator.model';
import { PingTree } from '../model/servicesModel/pingTree.model';

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
        public readonly end: number,
        public readonly manuallyCancelable: boolean,
        private cancellator: MassPingCancellator,
        public resetProgressCallback: () => void,
        public cancelCallback: () => void
    ) {
        if (cancellator !== null) {
            cancellator.onCancel = () => this.resetProgressCallback();
        }
    }

    public incremented(by: number): PingProgress {
        return new PingProgress(
            this.current + by,
            this.end,
            this.manuallyCancelable,
            this.cancellator,
            this.resetProgressCallback,
            this.cancelCallback
        );
    }

    public cancelMassPing() {
        if (this.manuallyCancelable) {
            this.cancelCallback();
            this.cancellator.cancel();
        }
    }

    get isCancellable(): boolean {
        return this.manuallyCancelable;
    }

    get isCancelled(): boolean {
        return this.cancellator.isCancelled;
    }
}

@Injectable()
export class MassPingService implements OnDestroy {
    private pingProgressSubject = new BehaviorSubject<PingProgress>(null);
    private isInProgress = false;
    private settingsSubscription: Subscription = null;
    private realTimePingUpdate = false;

    constructor(
        private pingCacheService: PingCacheService,
        private sounds: SoundNotificatonService,
        settingsService: SettingsProfilesService
    ) {
        this.settingsSubscription = settingsService.subscribeToSettingsChange(
            settings => {
                this.realTimePingUpdate = settings.profile.realTimePingUIUpdate;
        });
    }

    public ngOnDestroy() {
        if (this.settingsSubscription !== null) {
            this.settingsSubscription.unsubscribe();
        }
    }

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
        finishCallback: () => void,
        manuallyCancellable = true,
        cancellator = new MassPingCancellator()
    ) {
        if (this.setInProgress(
            nodesIDs.length,
            manuallyCancellable,
            cancellator,
            finishCallback
        )) {
            let counter = 0;
            this.pingCacheService.pingListByID(
                nodesIDs,
                this.realTimePingUpdate,
                (_) => {
                    counter++;
                    this.nextProgress(1);
                    if (counter === nodesIDs.length) {
                        this.resetInProgress();
                        finishCallback();
                    }
                },
                cancellator);
        }
    }

    public pingTreeWithoutCallback(
        roots: PingTree[],
        manuallyCancellable = true,
        cancellator = new MassPingCancellator()
    ) {
        this.pingTree(
            roots,
            (success, _1, _2) => {
                if (success === false) {
                    this.sounds.playFailedPingAlarm();
                }
            },
            null,
            manuallyCancellable,
            cancellator
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
        skipChildrenOfFailedNodes: boolean,
        manuallyCancellable = true,
        cancellator = new MassPingCancellator()
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
            manuallyCancellable,
            cancellator,
            skipChildrenOfFailedNodes
        );
    }

    private setInProgress(
        numParticipants: number,
        manuallyCancellable: boolean,
        cancellator: MassPingCancellator,
        finishedCallback: (result: TreePingFinishedData) => void
    ): boolean {
        if (this.isInProgress === true) {
            return false;
        }
        this.isInProgress = true;
        this.pingProgressSubject.next(
            new PingProgress(
                0,
                numParticipants,
                manuallyCancellable,
                cancellator,
                () => this.resetInProgress(),
                () => {
                    // will be called only on manual cancel (with cancel button)
                    if (finishedCallback !== null) {
                        finishedCallback(new TreePingFinishedData(0));
                    }
                }
            )
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
                current.incremented(numDone)
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
        manuallyCancellable: boolean,
        cancellator: MassPingCancellator,
        skipChildrenOfFailedNodes = true,
        tpfr: TreePingFinishedData = null
    ) {
        if (tpfr === null) {
            const count = this.countPingablePingTreeNodes(roots);
            if (this.setInProgress(
                count,
                manuallyCancellable,
                cancellator,
                finishedCallback
            )) {
                tpfr = new TreePingFinishedData(count);
            } else {
                return;
            }
        }
        const thisLayerPingDataList =
            this.preparePingDataLayer(roots);
        const thisLayerNodesIDs = thisLayerPingDataList.map(pd => {
            return pd.id;
        });
        let nextLayer: PingTree[] = [];
        let thisLayerDone = 0;
        this.pingCacheService.pingListByID(
            thisLayerNodesIDs,
            this.realTimePingUpdate,
            (idIndex: number, data: PingTestData) => {
                const success = data !== null
                && data.failed !== data.num;
                const pingTreeNode = thisLayerPingDataList[idIndex];
                const children = pingTreeNode.children;
                let skipped = 0;
                if (success) {
                    tpfr.successful++;
                    nextLayer = nextLayer.concat(children);
                    this.nextProgress(1);
                } else {
                    tpfr.failed++;
                    if (skipChildrenOfFailedNodes === true) {
                        skipped = this.countPingablePingTreeNodes(children);
                        tpfr.skipped += skipped;
                        this.pingCacheService.setFailedBranch(children);
                    }
                    this.nextProgress(1 + skipped);
                }
                if (forEachPingedCallback !== null) {
                    forEachPingedCallback(
                        success,
                        skipped,
                        pingTreeNode
                    );
                }
                thisLayerDone++;
                if (thisLayerDone === thisLayerNodesIDs.length
                && nextLayer.length > 0) {
                    this.pingTree(
                        nextLayer,
                        forEachPingedCallback,
                        finishedCallback,
                        manuallyCancellable,
                        cancellator,
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
            },
            cancellator
        );
    }
}
