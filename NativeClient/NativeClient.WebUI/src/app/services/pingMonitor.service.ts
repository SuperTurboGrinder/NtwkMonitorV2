import { Injectable } from '@angular/core';
import { SettingsProfilesService } from './settingsProfiles.service';
import { Subscription, Observable, timer, of } from 'rxjs';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';
import { NodesService } from './nodes.service';
import { map, switchMap } from 'rxjs/operators';
import { PingTree, PingCacheService } from './pingCache.service';
import { PingTreeBuilder } from '../ui/helpers/pingTreeBuilder.helper';
import { TreeTrimmingHelper } from '../ui/helpers/treeTrimmingHelper.helper';
import { MonitoringDataService } from './monitoringData.service';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { CWSData } from '../model/httpModel/cwsData.model';
import { NtwkNodesTree } from '../model/viewModel/ntwkNodesTree.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { CurrentMonitoringSessionDataService } from './currentMonitorSessionData.service';
import { SettingsProfile } from '../model/httpModel/settingsProfile.model';
import { MonitoringMessage } from '../model/httpModel/monitoringMessage.model';
import { MonitoringMessageType } from '../model/httpModel/monitoringMessageType.model';

@Injectable()
export class PingMonitorService {
    private settingsSubscription: Subscription = null;
    private monitorSubscription: Subscription = null;
    private settings: SettingsProfile;
    private endHour: number;
    private tagFilterData = new TagFilterData([], []);
    private isInitialized = false;
    private lastTickHourValue: number;

    private monitorPulseInProgress = false;
    private monitorPulse: MonitoringPulseResult;
    private monitorPulseNodesNumber: number;
    private monitorPulseProgress: number;

    public get isActive() {
        return this.monitorSubscription !== null;
    }

    constructor(
        private settingsService: SettingsProfilesService,
        private currentMonitorDataService: CurrentMonitoringSessionDataService,
        private nodesService: NodesService,
        private pingCache: PingCacheService
    ) {
        this.subscribeToSettingsChange();
    }

    private subscribeToSettingsChange() {
        if (this.settingsSubscription === null) {
            this.settingsSubscription =
                this.settingsService.subscribeToSettingsChange(settingsProfileData => {
                    this.settings = settingsProfileData.profile;
                    this.endHour = this.settings.monitoringStartHour
                        + this.settings.monitoringSessionDuration;
                    this.endHour = this.endHour === 24 ? 0 : this.endHour;
                    this.tagFilterData = settingsProfileData.monitorTagFilter;
                    if (this.isInitialized === false) {
                        this.isInitialized = true;
                        this.lastTickHourValue = new Date().getUTCHours();
                        if (this.settings.startMonitoringOnLaunch === true
                        && this.settings.monitoringStartHour <= this.lastTickHourValue) {
                            this.startMonitor();
                        }
                    }
            });
        }
    }

    private unsubscribeFromSettingsChange() {
        if (this.settingsSubscription !== null) {
            this.settingsSubscription.unsubscribe();
            this.settingsSubscription = null;
        }
    }

    public startMonitor() {
        if (this.monitorSubscription === null) {
            this.currentMonitorDataService.createNewSession(
                this.settings.id,
                sessionID => {
                    this.unsubscribeFromSettingsChange();
                    const monitorObservable: Observable<PingTree[]> =
                        this.nodesService.getNodesTree().pipe(
                            switchMap(treeResult =>
                                this.treeResultToMonitorSessionObservable(treeResult)
                            )
                        );
                    this.lastTickHourValue = new Date().getUTCHours();
                    this.monitorSubscription = monitorObservable
                        .subscribe(pt =>
                            this.monitorSubscriptionFunction(pt)
                        );
                }
            );
        }
    }

    public stopMonitor() {
        if (this.monitorSubscription !== null) {
            this.monitorSubscription.unsubscribe();
            this.monitorSubscription = null;
            this.subscribeToSettingsChange();
        }
    }

    private startNewPulseResult() {
        this.monitorPulseInProgress = true;
        this.monitorPulseProgress = 0;
        this.monitorPulse = new MonitoringPulseResult(
            0, 0, 0, 0, []
        );
    }

    private updatePulseResult(
        success: boolean,
        skippedNodesCount: number
    ) {
        if (success) {
            this.monitorPulse.responded++;
        } else {
            this.monitorPulse.silent++;
            this.monitorPulse.skipped += skippedNodesCount;
            const msg = new MonitoringMessage(
                skippedNodesCount === 0
                    ? MonitoringMessageType.Danger_NoPingReturned
                    : MonitoringMessageType.Danger_NoPingReturned_SkippedChildren,
                '',
                skippedNodesCount
            );
            this.monitorPulse.messages.push(msg);
        }
    }

    private finishPulseResult() {
        this.currentMonitorDataService.addNewPulse(
            this.monitorPulse
        );
        this.monitorPulse = null;
        this.monitorPulseInProgress = false;
    }

    private treeResultToMonitorSessionObservable(
        treeResult: HTTPResult<{
            cwsData: CWSData[],
            nodesTree: NtwkNodesTree
        }>
    ): Observable<PingTree[]> {
        if (treeResult.success === false
        || treeResult.data.nodesTree.treeLayers.length === 0) {
            return of([]);
        }
        const nodeTreeLayer0IDs = treeResult.data.nodesTree.treeLayers[0]
            .map(c => c.nodeData.node.id);
        const fullPingTree = PingTreeBuilder.buildAndFlatten(
            new TreeTrimmingHelper(treeResult.data.nodesTree.treeLayers, null)
                .getSubtree(0, treeResult.data.nodesTree.treeLayers.length)
        );
        const filteredPingTreeLayer0 = this.filterPingTree(
            fullPingTree
                .filter(ptRoot => nodeTreeLayer0IDs.includes(ptRoot.id))
        );
        const intervalMs = 1000 * 60 * this.settings.monitorInterval;
        this.monitorPulseNodesNumber =
            this.countPingTreeNodes(filteredPingTreeLayer0);
        this.startNewPulseResult();
        return timer(0, intervalMs).pipe(
            map(_ => filteredPingTreeLayer0)
        );
    }

    private monitorSubscriptionFunction(
        pingTree: PingTree[]
    ) {
        if (this.monitorPulseInProgress === false) {
            this.monitorPulseInProgress = true;
            const currentHour = new Date().getUTCHours();
            const isNewHour = this.lastTickHourValue < currentHour
                || (this.lastTickHourValue === 23 && currentHour === 0);
            this.lastTickHourValue = currentHour;
            if (isNewHour
                && currentHour === this.endHour) {
                this.stopMonitor();
                return;
            }
            this.pingCache.treeUpdateWithCallback(
                pingTree,
                (success, pingTreeNode) =>
                    this.treeNodeUpdateCallback(success, pingTreeNode)
                ,
                this.settings.depthMonitoring
            );
        }
    }

    private treeNodeUpdateCallback(
        success: boolean,
        pingTreeNode: PingTree
    ) {
        const successNodesCount = success ? 1 : 0;
        const failureNodesCount = success ? 0 : 1;
        const skippedNodesCount = (success
        && this.settings.depthMonitoring === true)
            ? 0
            : this.countPingTreeNodes(
                pingTreeNode.children
            );
        const summCount = successNodesCount
            + failureNodesCount + skippedNodesCount;
        this.monitorPulseProgress += summCount;
        /*this.updatePulseResult(
            success,
            pingTreeNode.
            skippedNodesCount
        );*/
        if (this.monitorPulseProgress
        === this.monitorPulseNodesNumber) {
            this.finishPulseResult();
        }
    }

    private filterPingTree(pingTreeBranch: PingTree[]): PingTree[] {
        return this.tagFilterData.nodesIDs.length === 0 || pingTreeBranch.length === 0
            ? pingTreeBranch
            : pingTreeBranch.map(
                subroot => this.tagFilterData.nodesIDs.includes(subroot.id)
                    ? [subroot].map(sr => {
                        sr.children = this.filterPingTree(sr.children);
                        return sr;
                    })
                    : this.filterPingTree(subroot.children)
            ).reduce((a, b) => a.concat(b), []);
    }

    private countPingTreeNodes(pingTreeBranch: PingTree[]): number {
        let counter = 0;
        for (const p of pingTreeBranch) {
            counter += 1 + this.countPingTreeNodes(p.children);
        }
        return counter;
    }
}
