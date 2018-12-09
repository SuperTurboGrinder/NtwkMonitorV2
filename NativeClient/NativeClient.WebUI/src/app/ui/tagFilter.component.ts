import { Component } from '@angular/core';
import { TagsService } from '../services/tags.service';
import { NodesService } from '../services/nodes.service';
import { SettingsProfilesService } from '../services/settingsProfiles.service';
import { NodeData } from '../model/httpModel/nodeData.model';
import { NodeInfoDataCache } from './helpers/nodesInfoDataCache.helper';
import { Observable, forkJoin } from 'rxjs';
import { NodeInfoPopupDataService } from '../services/nodeInfoPopupData.service';
import { NtwkNode } from '../model/httpModel/ntwkNode.model';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';
import { MessagingService } from '../services/messaging.service';
import { MessagesEnum } from '../model/servicesModel/messagesEnum.model';

@Component({
    selector: 'app-tag-filter',
    templateUrl: './tagFilter.component.html'
})
export class TagFilterComponent {
    private loadingError = false;
    private nodesListIsEmpty = false;
    private filteredViewNodesList: NodeData[] = null;
    private filteredMonitorNodesList: NodeData[] = null;
    private viewFilterData: TagFilterData = null;
    private monitorFilterData: TagFilterData = null;
    private viewNodesInfoPopupDataCache: NodeInfoDataCache;
    private monitorNodesInfoPopupDataCache: NodeInfoDataCache;
    private showingListFilter = true;

    public get isListFilter() {
        return this.showingListFilter;
    }

    public switchFilter() {
        this.showingListFilter = !this.showingListFilter;
    }

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
        return this.filteredMonitorNodesList;
    }

    public getViewTagsIDs(): number[] {
        return this.viewFilterData.tagsIDs;
    }

    public getMonitorTagsIDs(): number[] {
        return this.monitorFilterData.tagsIDs;
    }

    public deselectNode() {
        this.nodeInfoPopupService.setData(null);
    }

    public applyOperationsViewNodesChange(newTagsIDs: number[]) {
        this.filteredViewNodesList = null;
        this.loadingError = false;
        this.settingsService.setCurrentProfilesViewTags(newTagsIDs)
        .subscribe(success => {
            if (success) {
                this.messaging.showMessage(MessagesEnum.UpdatedSuccessfully);
            }
            this.refresh(true);
        });
    }

    public applyMonitoredNodesChange(newTagsIDs: number[]) {
        this.filteredViewNodesList = null;
        this.loadingError = false;
        this.settingsService.setCurrentProfilesMonitorTags(newTagsIDs)
        .subscribe(success => {
            if (success) {
                this.messaging.showMessage(MessagesEnum.UpdatedSuccessfully);
            }
            this.refresh(true);
        });
    }

    public refresh(_: boolean) {
        this.filteredViewNodesList = null;
        this.loadingError = false;
        this.initialize();
    }

    public get isLoadingError() {
        return this.loadingError;
    }

    constructor(
        private messaging: MessagingService,
        private nodeInfoPopupService: NodeInfoPopupDataService,
        private tagsService: TagsService,
        private nodesService: NodesService,
        private settingsService: SettingsProfilesService,
    ) {
        this.initialize();
    }

    private initialize() {
        this.nodesService.getNodesTree()
        .subscribe(nodeTreeResult => {
            if (nodeTreeResult.success === false) {
                this.loadingError = true;
                return;
            }
            forkJoin(
                this.settingsService.currentProfilesViewTagFilterData(),
                this.settingsService.currentProfilesMonitorTagFilterData()
            ).subscribe(results => {
                this.viewFilterData = results[0];
                this.monitorFilterData = results[1];
                const allNodes = nodeTreeResult.data.nodesTree.allNodes;
                const cwsData = nodeTreeResult.data.cwsData;
                const nodesNum = allNodes.length;
                if (nodesNum === 0) {
                    this.nodesListIsEmpty = true;
                    this.filteredViewNodesList = [null];
                } else {
                    this.nodesListIsEmpty = false;
                }
                this.filteredViewNodesList = allNodes
                    .filter(nData =>
                        this.viewFilterData.nodesIDs.includes(nData.nodeData.node.id)
                    )
                    .map(nData => nData.nodeData);
                this.filteredMonitorNodesList = allNodes
                    .filter(nData =>
                        this.monitorFilterData.nodesIDs.includes(nData.nodeData.node.id)
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
        });
    }
}
