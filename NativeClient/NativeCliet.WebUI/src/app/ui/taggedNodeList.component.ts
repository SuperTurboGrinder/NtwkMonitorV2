import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";

import { NtwkNode } from "../model/httpModel/ntwkNode.model";
import { CWSData } from "../model/httpModel/cwsData.model";
import { NodesService } from "../services/nodes.service";
import { NodeData } from "../model/httpModel/nodeData.model";
import { SettingsProfilesService } from "../services/settingsProfiles.service";
import { UpdateAlarmService } from "../services/updateAlarm.service";
import { PingCacheService } from "../services/pingCache.service";
import { NodeTag } from "../model/httpModel/nodeTag.model";
import { TagsService } from "../services/tags.service";
import { NodeInfoPopupDataService } from "../services/nodeInfoPopupData.service";
import { NodeInfoPopupData } from "../model/viewModel/nodeInfoPopupData.model";
import { NodeInfoDataCache } from "./helpers/nodesInfoDataCache.helper";

enum Sorting {
    Default,
    ByName,
    ByPing
}

@Component({
    selector: 'taggedNodeList',
    templateUrl: './taggedNodeList.component.html'
})
export class TaggedNodeListComponent implements OnDestroy {
    private filteredNodesList: NodeData[] = null;
    public sortedIndexes: number[] = null;
    private loadingError = false;
    private nodesTreeSubscription: Subscription = null;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;

    private sorting: Sorting = Sorting.Default;
    private sortingDescending: boolean = false;

    private defaultSortedIndexes(filteredNodesList: NodeData[], _: boolean) : number[] {
        return Array.from(
            {length: filteredNodesList.length},
            (_,i) => i
        )
    }

    private nameSortedIndexes(filteredNodesList: NodeData[], descending: boolean) : number[] {
        return filteredNodesList
            .map<{ i:number, name: string}>((nd, i) => { return {i:i, name:nd.node.name} })
            .sort((a, b) => !descending
                ? a.name.localeCompare(b.name)
                : b.name.localeCompare(a.name)
            )
            .map<number>(v => v.i);
    }

    private pingSortedIndexes(filteredNodesList: NodeData[], descending: boolean) : number[] {
        let unsortedFilteredNodesIndexes = new Map<number, number>(
            filteredNodesList
                .map<[number, number]>((nd, i) =>  [ nd.node.id, i ])
        );
        let sortedFilteredPingedNodesIndexes =
            this.pingCacheService.getIDsSortedByPing(descending)
                .filter(id => unsortedFilteredNodesIndexes.has(id))
                .map<number>(id => unsortedFilteredNodesIndexes.get(id));
        if(sortedFilteredPingedNodesIndexes.length == filteredNodesList.length)
            return sortedFilteredPingedNodesIndexes;
        let otherFilteredNodesIndexes: number[] = [];
        for(let i=0; i<filteredNodesList.length; i++) {
            if(!sortedFilteredPingedNodesIndexes.includes(i))
                otherFilteredNodesIndexes.push(i);
        }
        return sortedFilteredPingedNodesIndexes
            .concat(otherFilteredNodesIndexes);
    }

    public resortListByDefault() {
        this.sortedIndexes = this.defaultSortedIndexes(this.filteredNodesList, this.sortingDescending);
    }

    public resortListByName() {
        if(this.sorting != Sorting.ByName) return;
        this.sortedIndexes = this.nameSortedIndexes(this.filteredNodesList, this.sortingDescending);
    }

    public resortListByPing() {
        if(this.sorting != Sorting.ByPing) return;
        this.sortedIndexes = this.pingSortedIndexes(this.filteredNodesList, this.sortingDescending);
    }

    private resortIndexList() {
        switch(this.sorting) {
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
        let isSortedBy = this.sorting === srt;
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

    listWebServicesData(i:number) : {name:string, id:number}[] {
        return this.nodeInfoPopupDataCache.listWebServicesData(
            i,
            this.filteredNodesList[i].boundWebServicesIDs
        );
    }

    public selectNode(i:number, event:MouseEvent) {;
        this.nodeInfoPopupService.setData(
            this.nodeInfoPopupDataCache.formNodeInfoPopupData(
                i,
                this.filteredNodesList[i],
                {x: event.clientX, y: event.clientY}
            )
        );
    }

    public deselectNode(event:MouseEvent) {
        this.nodeInfoPopupService.setData(null);
    }

    public node(i:number) : NtwkNode {
        return this.filteredNodesList[i].node;
    }

    

    public get isLoadingError() {
        return this.loadingError;
    }

    nodeTrackByFn(index:number, node_index:number) {
        if(!this.filteredNodesList) return null;
        return this.filteredNodesList[node_index].node.id;
    }

    public refresh(_: boolean) {
        this.updateList();
    }

    private updateList() {
        this.loadingError = false;
        this.updateService.sendUpdateNodesAndTagsAlarm();
    }

    constructor(
        private pingCacheService: PingCacheService,
        private updateService: UpdateAlarmService,
        private nodeInfoPopupService: NodeInfoPopupDataService,
        tagsService: TagsService,
        nodesService: NodesService,
        settingsService: SettingsProfilesService,
    ) {
        this.nodesTreeSubscription = nodesService.getNodesTree().subscribe(treeResult => {
            if(treeResult === null) {
                return;
            } else if(treeResult.success === false) {
                this.loadingError = true;
                return;
            }
            settingsService.currentProfilesViewNodesIDs()
                .subscribe(viewNodesIDsResult => {
                    if(viewNodesIDsResult.success === false) {
                        this.loadingError = true;
                        return;
                    }
                    let viewNodesIDs = viewNodesIDsResult.data;
                    let allNodes = treeResult.data.nodesTree.allNodes;
                    this.filteredNodesList = allNodes
                        .filter(nData => 
                            viewNodesIDs.includes(nData.nodeData.node.id)
                        )
                        .map(nData => nData.nodeData);
                    this.renewPopupDataCache(
                        new NodeInfoDataCache(
                            this.filteredNodesList.length,
                            treeResult.data.cwsData,
                            tagsService
                        )
                    );
                    this.loadingError = this.loadingError && this.nodeInfoPopupDataCache.loadingError;
                    this.sortedIndexes = this.defaultSortedIndexes(this.filteredNodesList, false);
                })
        });
    }

    private renewPopupDataCache(newCache: NodeInfoDataCache) {
        if(this.nodeInfoPopupDataCache != null) {
            this.nodeInfoPopupDataCache.destroy();
        }
        this.nodeInfoPopupDataCache = newCache;
    }

    ngOnDestroy() {
        this.renewPopupDataCache(null);
        this.nodesTreeSubscription.unsubscribe();
    }
}