import { Component, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';

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
    templateUrl: './taggedNodeList.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaggedNodeListComponent {
    private filteredNodesList: NodeData[] = null;
    public sortedIndexes: number[] = null;
    private loadingError = false;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;
    private nodesListIsEmpty = true;
    private isOperationsView = false;
    private filteredNodesListPingInProgress = false;
    private stringCollator = new Intl.Collator(undefined, {numeric: true, sensitivity: 'base'});

    private sorting: Sorting = Sorting.Default;
    private sortingDescending = false;

    private nameSortedIndexesCacheD: number[] = null;
    private nameSortedIndexesCacheA: number[] = null;
    private ipSortedIndexesCache: number[] = null;

    @Output() private updateUIEvent = new EventEmitter();
    private updateUI() {
        this.updateUIEvent.emit();
    }

    public get isFilterMode() {
        return this.isOperationsView === false;
    }

    public get isWholeListPingInProgress() {
        return this.filteredNodesListPingInProgress;
    }

    public switchToOperationsMode() {
        this.isOperationsView = true;
    }

    public switchToFilterMode() {
        this.isOperationsView = false;
    }

    private ipSortedIndexes() { // allways descending
        if (this.ipSortedIndexesCache !== null) {
            return this.ipSortedIndexesCache;
        } else {
            this.ipSortedIndexesCache = this.filteredNodesList
                .map((nd, i) => ({ i: i, ipParts: nd.node.ipStr.split( '.' ) }))
                .sort((a, b) => {
                    for (let i = 0; i < 4; i++ ) {
                        const ipA = parseInt(a.ipParts[i], 10);
                        const ipB = parseInt(b.ipParts[i], 10);
                        if (ipA === ipB) {
                            continue;
                        } else {
                            return ipA < ipB ? -1 : 1;
                        }
                    }
                    return 0;
                })
                .map(d => d.i);
            return this.ipSortedIndexesCache;
        }
    }

    private nameSortedIndexes(): number[] {
        let cache = this.sortingDescending
            ? this.nameSortedIndexesCacheD
            : this.nameSortedIndexesCacheA;
        if (cache !== null) {
            return cache;
        } else {
            cache = this.filteredNodesList
                .map<{ i: number, name: string}>(
                    (nd, i) => ({i: i, name: nd.node.name})
                )
                .sort((a, b) => !this.sortingDescending
                    ? this.stringCollator.compare(a.name, b.name)
                    : this.stringCollator.compare(b.name, a.name)
                )
                .map<number>(v => v.i);
            if (this.sortingDescending) {
                this.nameSortedIndexesCacheD = cache;
            } else {
                this.nameSortedIndexesCacheA = cache;
            }
            return cache;
        }
    }

    private pingSortedIndexes(): number[] {
        const unsortedFilteredNodesIndexes = new Map<number, number>(
            this.filteredNodesList
                .map<[number, number]>((nd, i) =>  [ nd.node.id, i ])
        );
        const sortedFilteredPingedNodesIndexes =
            this.pingCacheService.getIDsSortedByPing(this.sortingDescending)
                .filter(id => unsortedFilteredNodesIndexes.has(id))
                .map<number>(id => unsortedFilteredNodesIndexes.get(id));
        if (sortedFilteredPingedNodesIndexes.length === this.filteredNodesList.length) {
            return sortedFilteredPingedNodesIndexes;
        }
        const otherFilteredNodesIndexes: number[] = [];
        for (let i = 0; i < this.filteredNodesList.length; i++) {
            if (!sortedFilteredPingedNodesIndexes.includes(i)) {
                otherFilteredNodesIndexes.push(i);
            }
        }
        return sortedFilteredPingedNodesIndexes
            .concat(otherFilteredNodesIndexes);
    }

    public resortListByDefault() {
        this.sortedIndexes = this.ipSortedIndexes();
    }

    public resortListByName() {
        if (this.sorting !== Sorting.ByName) {
            return;
        }
        this.sortedIndexes = this.nameSortedIndexes();
    }

    private resortListByPing() {
        if (this.sorting !== Sorting.ByPing
             || this.filteredNodesListPingInProgress === true
        ) {
            return;
        }
        this.sortedIndexes = this.pingSortedIndexes();
    }

    public resortListByPingIfChanged(isChanged: boolean) {
        if (isChanged === true) {
            this.resortListByPing();
        }
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
        this.filteredNodesListPingInProgress = true;
        this.pingCacheService.updateValues(
            this.filteredNodesList
                .filter(n => n.node.isOpenPing === true)
                .map(n => n.node.id),
            () => {
                this.filteredNodesListPingInProgress = false;
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
        private settingsService: SettingsProfilesService
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
            if (treeResult.success === false) {
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
            const viewNodesIDs = viewTagFilterDataResult.nodesIDs;
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
            this.sortedIndexes = this.ipSortedIndexes();
            this.updateUI();
        });
    }
}
