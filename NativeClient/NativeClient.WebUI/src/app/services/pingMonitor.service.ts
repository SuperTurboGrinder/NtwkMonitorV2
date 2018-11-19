import { Injectable } from '@angular/core';
import { SettingsProfilesService } from './settingsProfiles.service';
import { Subscription, Observable, timer, of } from 'rxjs';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';
import { NodesService } from './nodes.service';
import { map, switchMap } from 'rxjs/operators';
import { PingTree, PingCacheService, TreePingFinishedData } from './pingCache.service';
import { PingTreeBuilder } from '../ui/helpers/pingTreeBuilder.helper';
import { TreeTrimmingHelper } from '../ui/helpers/treeTrimmingHelper.helper';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { CWSData } from '../model/httpModel/cwsData.model';
import { NtwkNodesTree } from '../model/viewModel/ntwkNodesTree.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { CurrentMonitoringSessionDataService } from './currentMonitorSessionData.service';
import { SettingsProfile } from '../model/httpModel/settingsProfile.model';
import { MonitoringMessage } from '../model/httpModel/monitoringMessage.model';
import { MonitoringMessageType } from '../model/httpModel/monitoringMessageType.model';
import { ManualTimer } from '../ui/helpers/manualTimer.helper';

@Injectable()
export class PingMonitorService {
    private settingsSubscription: Subscription = null;
    private monitorSubscription: Subscription = null;
    private settings: SettingsProfile;
    private endHour: number;
    private tagFilterData = new TagFilterData([], []);
    private isInitialized = false;
    private lastTickHourValue: number;
    private monitorManualTimer: ManualTimer = null;

    private monitorPulseInProgress = false;
    private monitorPulseMessages: MonitoringMessage[] = [];

    public get isActive() {
        return this.monitorSubscription !== null;
    }

    public get isPulsing() {
        return this.monitorPulseInProgress === true;
    }

    public get toNextPulse() {
        return this.monitorManualTimer === null
            ? null
            : this.monitorManualTimer.getMsToReset();
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
                () => {
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
            this.monitorManualTimer = null;
            this.subscribeToSettingsChange();
        }
    }

    private addFailedPingMessage(
        skipped: number,
        name: string
    ) {
        const msg = new MonitoringMessage(
            skipped === 0
                ? MonitoringMessageType.Danger_NoPingReturned
                : MonitoringMessageType.Danger_NoPingReturned_SkippedChildren,
            name,
            skipped
        );
        this.monitorPulseMessages.push(msg);
    }

    private resetPulseData() {
        this.monitorPulseMessages = [];
        this.monitorPulseInProgress = false;
    }

    private savePulseData(pingFinishedData: TreePingFinishedData) {
        const pulse = new MonitoringPulseResult(
            pingFinishedData.successful,
            pingFinishedData.failed,
            pingFinishedData.skipped,
            0,
            this.monitorPulseMessages
        );
        this.currentMonitorDataService.addNewPulse(pulse);
        this.resetPulseData();
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
        this.monitorManualTimer = new ManualTimer(intervalMs);
        this.resetPulseData();
        return timer(0, 1000).pipe(
            map(_ => filteredPingTreeLayer0)
        );
    }

    private monitorSubscriptionFunction(
        pingTree: PingTree[]
    ) {
        if (this.monitorManualTimer.checkIfFinished() === false) {
            return;
        }
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
                (success, skipped, pingTreeNode) =>
                    this.treeNodeUpdateCallback(success, skipped, pingTreeNode)
                ,
                pingFinishedResult => {
                    this.savePulseData(pingFinishedResult);
                },
                this.settings.depthMonitoring
            );
        }
    }

    private treeNodeUpdateCallback(
        success: boolean,
        skipped: number,
        pingTreeNode: PingTree
    ) {
        if (success === false) {
            this.addFailedPingMessage(
                skipped,
                pingTreeNode.name
            );
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
}
