import { Injectable } from '@angular/core';
import { SettingsProfilesService } from './settingsProfiles.service';
import { Subscription, Observable, timer, of } from 'rxjs';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';
import { NodesService } from './nodes.service';
import { map, switchMap } from 'rxjs/operators';
import { PingTree, PingCacheService } from './pingCache.service';
import { PingTreeBuilder } from '../ui/helpers/pingTreeBuilder.helper';
import { TreeTrimmingHelper } from '../ui/helpers/treeTrimmingHelper.helper';

@Injectable()
export class PingMonitorService {
    private settingsSubscription: Subscription = null;
    private monitorSubscription: Subscription = null;
    private startHour: number;
    private endHour: number;
    private monitorInterval: number;
    private startOnLaunch: boolean;
    private depthMonitoring: boolean;
    private tagFilterData = new TagFilterData([], []);
    private isInitialized = false;
    private lastTickHourValue: number;

    public get isActive() {
        return this.monitorSubscription !== null;
    }

    constructor(
        private settingsService: SettingsProfilesService,
        private nodesService: NodesService,
        private pingCache: PingCacheService
    ) {
        this.subscribeToSettingsChange();
    }

    private subscribeToSettingsChange() {
        if (this.settingsSubscription === null) {
            this.settingsSubscription =
                this.settingsService.subscribeToSettingsChange(settingsProfileData => {
                    const settingsProfile = settingsProfileData.profile;
                    this.startHour = settingsProfile.monitoringStartHour;
                    this.endHour = this.startHour + settingsProfile.monitoringSessionDuration;
                    this.endHour = this.endHour === 24 ? 0 : this.endHour;
                    this.monitorInterval = settingsProfile.monitorInterval;
                    this.startOnLaunch = settingsProfile.startMonitoringOnLaunch;
                    this.depthMonitoring = settingsProfile.depthMonitoring;
                    this.tagFilterData = settingsProfileData.monitorTagFilter;
                    if (this.isInitialized === false) {
                        this.isInitialized = true;
                        this.lastTickHourValue = new Date().getUTCHours();
                        if (this.startOnLaunch === true
                            && this.startHour <= this.lastTickHourValue) {
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
            this.unsubscribeFromSettingsChange();
            const monitorObservable: Observable<PingTree[]> = this.nodesService.getNodesTree().pipe(
                switchMap(treeResult => {
                    if (treeResult.success === false || treeResult.data.nodesTree.treeLayers.length === 0) {
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
                    const intervalMs = 1000 * 60 * this.monitorInterval;
                    return timer(0, intervalMs).pipe(
                        map(_ => filteredPingTreeLayer0)
                    );
                })
            );
            this.lastTickHourValue = new Date().getUTCHours();
            this.monitorSubscription = monitorObservable
                .subscribe(pt => {
                    const currentHour = new Date().getUTCHours();
                    const isNewHour = this.lastTickHourValue < currentHour
                        || (this.lastTickHourValue === 23 && currentHour === 0);
                    this.lastTickHourValue = currentHour;
                    if (isNewHour
                        && currentHour === this.endHour) {
                        this.stopMonitor();
                        return;
                    }
                    this.pingCache.noMessagesTreeUpdate(pt);
                });
        }
    }

    public stopMonitor() {
        if (this.monitorSubscription !== null) {
            this.monitorSubscription.unsubscribe();
            this.monitorSubscription = null;
            this.subscribeToSettingsChange();
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
