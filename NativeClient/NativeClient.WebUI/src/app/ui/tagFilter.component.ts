import { Component } from '@angular/core';
import { TagsService } from '../services/tags.service';
import { NodesService } from '../services/nodes.service';
import { SettingsProfilesService } from '../services/settingsProfiles.service';
import { NodeData } from '../model/httpModel/nodeData.model';
import { NodeInfoDataCache } from './helpers/nodesInfoDataCache.helper';
import { Observable, forkJoin } from 'rxjs';
import { NodeInfoPopupDataService } from '../services/nodeInfoPopupData.service';
import { NtwkNode } from '../model/httpModel/ntwkNode.model';

@Component({
    selector: 'app-tag-filter',
    templateUrl: './tagFilter.component.html'
})
export class TagFilterComponent {
    private loadingError = false;
    private nodesListIsEmpty = false;
    private filteredViewNodesList: NodeData[] = null;
    private filteredMonitorNodesList: NodeData[] = null;
    private viewNodesIDs: number[] = [];
    private monitorNodesIDs: number[] = [];
    private viewNodesInfoPopupDataCache: NodeInfoDataCache;
    private monitorNodesInfoPopupDataCache: NodeInfoDataCache;

    public isNoNodesLoaded(): boolean {
        return this.nodesListIsEmpty;
    }

    private selectNode(
        cache: NodeInfoDataCache,
        nodes: NodeData[],
        i: number,
        event: MouseEvent
    ) {
        this.nodeInfoPopupService.setData(
            cache.formNodeInfoPopupData(
                i,
                nodes[i],
                {x: event.clientX, y: event.clientY}
            )
        );
    }

    public selectViewNode(i: number, event: MouseEvent) {
        this.selectNode(
            this.viewNodesInfoPopupDataCache,
            this.filteredViewNodesList,
            i,
            event
        );
    }

    public selectMonitorNode(i: number, event: MouseEvent) {
        this.selectNode(
            this.monitorNodesInfoPopupDataCache,
            this.filteredMonitorNodesList,
            i,
            event
        );
    }

    public get viewNodes(): NodeData[] {
        return this.filteredViewNodesList;
    }

    public get monitorNodes(): NodeData[] {
        return this.filteredViewNodesList;
    }

    public deselectNode() {
        this.nodeInfoPopupService.setData(null);
    }

    constructor(
        private nodeInfoPopupService: NodeInfoPopupDataService,
        private tagsService: TagsService,
        private nodesService: NodesService,
        private settingsService: SettingsProfilesService,
    ) {
        this.initialize();
    }

    private initialize() {
        forkJoin(
            this.nodesService.getNodesTree(),
            this.settingsService.currentProfilesViewNodesIDs(),
            this.settingsService.currentProfilesMonitorNodesIDs()
        ).subscribe(results => {
            this.viewNodesIDs = results[1].data;
            this.monitorNodesIDs = results[2].data;
            const allNodes = results[0].data.nodesTree.allNodes;
            const cwsData = results[0].data.cwsData;
            const nodesNum = allNodes.length;
            if (
                results[0].success === false
                || results[1].success === false
                || results[2].success === false
            ) {
                this.loadingError = true;
                return;
            }
            if (nodesNum === 0) {
                this.nodesListIsEmpty = true;
                this.filteredViewNodesList = [null];
            } else {
                this.nodesListIsEmpty = false;
            }
            this.filteredViewNodesList = allNodes
                .filter(nData =>
                    this.viewNodesIDs.includes(nData.nodeData.node.id)
                )
                .map(nData => nData.nodeData);
            this.filteredMonitorNodesList = allNodes
                .filter(nData =>
                    this.monitorNodesIDs.includes(nData.nodeData.node.id)
                )
                .map(nData => nData.nodeData);
            this.viewNodesInfoPopupDataCache =
                new NodeInfoDataCache(
                    this.filteredViewNodesList.length,
                    cwsData,
                    this.tagsService
                );
            this.monitorNodesInfoPopupDataCache =
                new NodeInfoDataCache(
                    this.filteredMonitorNodesList.length,
                    cwsData,
                    this.tagsService
                );
            this.loadingError = this.loadingError
                && this.viewNodesInfoPopupDataCache.loadingError
                && this.monitorNodesInfoPopupDataCache.loadingError;
        });
    }
}
