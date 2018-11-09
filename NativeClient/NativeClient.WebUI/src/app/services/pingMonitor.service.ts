import { Injectable } from '@angular/core';
import { SettingsProfilesService } from './settingsProfiles.service';
import { Subscription, Observable, timer } from 'rxjs';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';
import { NodesService } from './nodes.service';
import { map } from 'rxjs/operators';
import { PingTree } from './pingCache.service';
import { PingTreeBuilder } from '../ui/helpers/pingTreeBuilder.helper';
import { TreeTrimmingHelper } from '../ui/helpers/treeTrimmingHelper.helper';

@Injectable()
export class PingMonitorService {
    private isActive = false;
    private settingsSubscription: Subscription = null;
    private startHour: number;
    private sessionDuration: number;
    private monitorInterval: number;
    private startOnLaunch: boolean;
    private depthMonitoring: boolean;
    private tagFilterData = new TagFilterData([], []);
    private isInitialized: boolean;
    private monitorObservable: Observable<number> = null;

    constructor(
        private settingsService: SettingsProfilesService,
        private nodesService: NodesService
    ) {
        this.subscribeToSettingsChange();
    }

    private subscribeToSettingsChange() {
        if (this.settingsSubscription !== null) {
            this.unsubscribeFromSettingsChange();
        }
        this.settingsSubscription =
            this.settingsService.subscribeToSettingsChange(settingsProfileData => {
                const settingsProfile = settingsProfileData.profile;
                this.startHour = settingsProfile.monitoringStartHour;
                this.sessionDuration = settingsProfile.monitoringSessionDuration;
                this.monitorInterval = settingsProfile.monitorInterval;
                this.startOnLaunch = settingsProfile.startMonitoringOnLaunch;
                this.depthMonitoring = settingsProfile.depthMonitoring;
                this.tagFilterData = settingsProfileData.monitorTagFilter;
                if (this.isInitialized === false) {
                    this.isInitialized = true;
                    if (this.startOnLaunch === true
                        && this.startHour <= new Date().getUTCHours()) {
                            this.startMonitor();
                        }
                }
        });
    }

    private unsubscribeFromSettingsChange() {
        this.settingsSubscription.unsubscribe();
        this.settingsSubscription = null;
    }

    public startMonitor() {
        if (this.isActive === true) {
            return;
        }
        this.unsubscribeFromSettingsChange();
        this.isActive = true;
        const pingTreeObservable = this.nodesService.getNodesTree().pipe(
            map(treeResult => {
                if (treeResult.success === false) {
                    return null;
                }
                const fullPingTree = PingTreeBuilder.buildAndFlatten(
                    new TreeTrimmingHelper(treeResult.data.nodesTree.treeLayers, null)
                        .getSubtree(0, treeResult.data.nodesTree.treeLayers.length - 1)
                );
            })
        )
        this.monitorObservable = timer().pipe(
            map(n => {
                const nodesPingTree: PingTree = null;
                return ({n: n, pingTree: nodesPingTree});
            })
        );
        this.monitorObservable.subscribe(_ => {
            ;
        });
    }

    private filterPingTree(dirtyPingTree: PingTree[]): PingTree[] {
        return dirtyPingTree
            .filter(pt => this.tagFilterData.nodesIDs.includes(pt.id))
            .map(pt => ({
                id: pt.id,
                isPingable: pt.isPingable,
                isBranchPingable: pt.isBranchPingable,
                children: this.pingTreeDepthCleaning(pt.children)
            }));
    }

    private pingTreeDepthCleaning(pingTreeChildren: PingTree[]): PingTree[] {

    }
}
