import { Component } from '@angular/core';

import { NtwkNode } from '../model/httpModel/ntwkNode.model';
import { NodesService } from '../services/nodes.service';
import { NodeData } from '../model/httpModel/nodeData.model';
import { SettingsProfilesService } from '../services/settingsProfiles.service';
import { PingCacheService } from '../services/pingCache.service';
import { TagsService } from '../services/tags.service';
import { NodeInfoPopupDataService } from '../services/nodeInfoPopupData.service';
import { NodeInfoDataCache } from './helpers/nodesInfoDataCache.helper';
import { forkJoin } from 'rxjs';

enum Sorting {
    Default,
    ByName,
    ByPing
}

@Component({
    selector: 'app-tagged-node-list',
    templateUrl: './taggedNodeList.component.html'
})
export class TaggedNodeListComponent {
    private filteredNodesList: NodeData[] = null;
    public sortedIndexes: number[] = null;
    private loadingError = false;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;
    private nodesListIsEmpty = true;
    private isOperationsView = false;
    private sortingByPingBlocked = false;

    private sorting: Sorting = Sorting.Default;
    private sortingDescending = false;

    public get isFilterMode() {
        return this.isOperationsView === false;
    }

    public switchToOperationsMode() {
        this.isOperationsView = true;
    }

    public switchToFilterMode() {
        this.isOperationsView = false;
    }

    private defaultSortedIndexes(filteredNodesList: NodeData[], _: boolean): number[] {
        return Array.from(
            {length: filteredNodesList.length},
            (__, i) => i
        );
    }

    private nameSortedIndexes(filteredNodesList: NodeData[], descending: boolean): number[] {
        return filteredNodesList
            .map<{ i: number, name: string}>(
                (nd, i) => ({i: i, name: nd.node.name})
            )
            .sort((a, b) => !descending
                ? a.name.localeCompare(b.name)
                : b.name.localeCompare(a.name)
            )
            .map<number>(v => v.i);
    }

    private pingSortedIndexes(filteredNodesList: NodeData[], descending: boolean): number[] {
        const unsortedFilteredNodesIndexes = new Map<number, number>(
            filteredNodesList
                .map<[number, number]>((nd, i) =>  [ nd.node.id, i ])
        );
        const sortedFilteredPingedNodesIndexes =
            this.pingCacheService.getIDsSortedByPing(descending)
                .filter(id => unsortedFilteredNodesIndexes.has(id))
                .map<number>(id => unsortedFilteredNodesIndexes.get(id));
        if (sortedFilteredPingedNodesIndexes.length === filteredNodesList.length) {
            return sortedFilteredPingedNodesIndexes;
        }
        const otherFilteredNodesIndexes: number[] = [];
        for (let i = 0; i < filteredNodesList.length; i++) {
            if (!sortedFilteredPingedNodesIndexes.includes(i)) {
                otherFilteredNodesIndexes.push(i);
            }
        }
        return sortedFilteredPingedNodesIndexes
            .concat(otherFilteredNodesIndexes);
    }

    public resortListByDefault() {
        this.sortedIndexes = this.defaultSortedIndexes(this.filteredNodesList, this.sortingDescending);
    }

    public resortListByName() {
        if (this.sorting !== Sorting.ByName) {
            return;
        }
        this.sortedIndexes = this.nameSortedIndexes(this.filteredNodesList, this.sortingDescending);
    }

    public resortListByPing() {
        if (this.sorting !== Sorting.ByPing
             || this.sortingByPingBlocked === true
        ) {
            return;
        }
        this.sortedIndexes = this.pingSortedIndexes(this.filteredNodesList, this.sortingDescending);
    }

    private resortIndexList() {
        switch (this.sorting) {
            case Sorting.Default:
                this.resortListByDefault();
            break;
            case Sorting.ByName:
                this.resortListByName();
            break;
            case Sorting.ByPing:
                this.resortListByPing();
            break;
        }
    }

    private setSortedBy(srt: Sorting) {
        const isSortedBy = this.sorting === srt;
        this.sorting = !isSortedBy || !this.sortingDescending
            ? srt : Sorting.Default;
        this.sortingDescending = isSortedBy && !this.sortingDescending
            ? true : false;
        this.resortIndexList();
    }

    public setSortByName() {
        this.setSortedBy(Sorting.ByName);
    }

    public setSortByPing() {
        this.setSortedBy(Sorting.ByPing);
    }

    public isSortedByName() {
        return this.sorting === Sorting.ByName;
    }

    public isSortedByPing() {
        return this.sorting === Sorting.ByPing;
    }

    public isSortedDescending() {
        return this.sortingDescending;
    }

    listWebServicesData(i: number): {name: string, id: number}[] {
        return this.nodeInfoPopupDataCache.listWebServicesData(
            i,
            this.filteredNodesList[i].boundWebServicesIDs
        );
    }

    public selectNode(i: number, event: MouseEvent) {
        this.nodeInfoPopupService.setData(
            this.nodeInfoPopupDataCache.formNodeInfoPopupData(
                i,
                this.filteredNodesList[i],
                {x: event.clientX, y: event.clientY}
            )
        );
    }

    public deselectNode() {
        this.nodeInfoPopupService.setData(null);
    }

    public node(i: number): NtwkNode {
        return this.filteredNodesList[i].node;
    }

    public isNoNodesLoaded(): boolean {
        return this.nodesListIsEmpty;
    }

    public get isLoadingError() {
        return this.loadingError;
    }

    public pingFilteredList() {
        this.sortingByPingBlocked = true;
        this.pingCacheService.updateValues(
            this.filteredNodesList
                .filter(n => n.node.isOpenPing === true)
                .map(n => n.node.id),
            () => {
                this.sortingByPingBlocked = false;
                if (this.sorting === Sorting.ByPing) {
                    this.resortListByPing();
                }
            }
        );
    }

    nodeTrackByFn(node_index: number) {
        if (!this.filteredNodesList) {
            return null;
        }
        return this.filteredNodesList[node_index].node.id;
    }

    public refresh(_: boolean) {
        this.loadingError = false;
        this.initialize();
    }

    constructor(
        private pingCacheService: PingCacheService,
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
            this.settingsService.currentProfilesViewTagFilterData()
        )
        .subscribe(results => {
            const treeResult = results[0];
            const viewTagFilterDataResult = results[1];
            if (treeResult.success === false
            || viewTagFilterDataResult.success === false) {
                this.loadingError = true;
                return;
            }
            const nodesNum = treeResult.data.nodesTree.allNodes.length;
            if (nodesNum === 0) {
                this.nodesListIsEmpty = true;
                this.filteredNodesList = [null];
            } else {
                this.nodesListIsEmpty = false;
            }
            const viewNodesIDs = viewTagFilterDataResult.data.nodesIDs;
            const allNodes = treeResult.data.nodesTree.allNodes;
            this.filteredNodesList = allNodes
                .filter(nData =>
                    viewNodesIDs.includes(nData.nodeData.node.id)
                )
                .map(nData => nData.nodeData);
            this.nodeInfoPopupDataCache =
                new NodeInfoDataCache(
                    this.filteredNodesList.length,
                    treeResult.data.cwsData,
                    this.tagsService
                );
            this.loadingError = this.loadingError && this.nodeInfoPopupDataCache.loadingError;
            this.sortedIndexes = this.defaultSortedIndexes(this.filteredNodesList, false);
        });
    }
}
