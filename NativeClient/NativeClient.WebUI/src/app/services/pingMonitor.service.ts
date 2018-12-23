import { Injectable } from '@angular/core';
import { SettingsProfilesService } from './settingsProfiles.service';
import { Subscription, Observable, timer, of } from 'rxjs';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';
import { NodesService } from './nodes.service';
import { map, switchMap, mapTo } from 'rxjs/operators';
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
import { MassPingService, TreePingFinishedData } from './massPing.service';
import { MassPingCancellator } from '../model/servicesModel/massPingCancelator.model';
import { PingTree } from '../model/servicesModel/pingTree.model';

@Injectable()
export class PingMonitorService {
    private settingsSubscription: Subscription = null;
    private monitorSubscription: Subscription = null;
    private settings: SettingsProfile;
    private tagFilterData = new TagFilterData([], []);
    private isInitialized = false;
    private lastTickDay: number;
    private monitorManualTimer: ManualTimer = null;
    private cancellator: MassPingCancellator = null;

    private monitorPulseInProgress = false;
    private monitorPulseMessages: MonitoringMessage[] = [];

    public get isReady() {
        return this.isInitialized;
    }

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
        private massPingService: MassPingService
    ) {
        this.subscribeToSettingsChange();
    }

    private subscribeToSettingsChange() {
        if (this.settingsSubscription === null) {
            this.settingsSubscription =
                this.settingsService.subscribeToSettingsChange(settingsProfileData => {
                    this.settings = settingsProfileData.profile;
                    this.tagFilterData = settingsProfileData.monitorTagFilter;
                    if (this.isInitialized === false) {
                        this.isInitialized = true;
                        this.lastTickDay = new Date().getDate();
                        if (this.settings.startMonitoringOnLaunch === true) {
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
                    this.lastTickDay = new Date().getDate();
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
            if (this.cancellator !== null) {
                this.cancellator.cancel();
                this.cancellator = null;
            }
            this.resetPulseData();
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
                .filter(
                    ptRoot => ptRoot !== null
                    && nodeTreeLayer0IDs.includes(ptRoot.id)
                )
        );
        const intervalMs = 1000 * 60 * this.settings.monitorInterval;
        this.monitorManualTimer = new ManualTimer(intervalMs);
        this.resetPulseData();
        return timer(0, 1000).pipe(
            mapTo(filteredPingTreeLayer0)
        );
    }

    private monitorSubscriptionFunction(
        pingTree: PingTree[]
    ) {
        if (this.monitorManualTimer.checkIfFinished() === false
         || this.massPingService.inProgress === true) {
            return;
        }
        if (this.monitorPulseInProgress === false) {
            this.monitorPulseInProgress = true;
            const currentDay = new Date().getDate();
            const isNewDay = this.lastTickDay !== currentDay;
            this.lastTickDay = currentDay;
            // reset session after midnight
            if (isNewDay) {
                this.stopMonitor();
                this.startMonitor();
                return;
            }
            this.cancellator = new MassPingCancellator();
            this.massPingService.pingTreeWithCallback(
                pingTree,
                (success, skipped, pingTreeNode) =>
                    this.treeNodeUpdateCallback(success, skipped, pingTreeNode),
                pingFinishedResult => {
                    this.savePulseData(pingFinishedResult);
                },
                this.settings.depthMonitoring,
                false,
                this.cancellator
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
