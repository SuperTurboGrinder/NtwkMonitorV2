import { Component } from "@angular/core";

import { NtwkNode } from "../model/httpModel/ntwkNode.model";
import { NodesService } from "../services/nodes.service";
import { PingCacheService, PingTree } from "../services/pingCache.service";
import { TagsService } from "../services/tags.service";
import { NodeInfoPopupDataService } from "../services/nodeInfoPopupData.service";
import { NodeInfoDataCache } from "./helpers/nodesInfoDataCache.helper";
import { TreeCollapsingService } from "../services/treeCollapsing.service";
import { Range } from "../ui/misc/numRangeSelector.component";
import { DisplayTreeHelper } from "./helpers/displayTreeHelper.helper";

@Component({
    selector: 'nodesTreeView',
    templateUrl: './nodesTreeView.component.html'
})
export class NodesTreeViewComponent {
    private displayTreeHelper: DisplayTreeHelper = null;
    private loadingError = false;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;
    private nodesListIsEmpty: boolean = true;
    private initialized: boolean = false;

    private readonly maxDisplayedLayers = 10;
    private startDisplayLayer = 0;
    private displayLayersCount = 0;
    private treeLayersCount = 0;

    public prefix(prefixIndex: number) {
        return this.displayTreeHelper.prefix(prefixIndex);
    }
    
    listWebServicesData(i:number) : {name:string, id:number}[] {
        return this.nodeInfoPopupDataCache.listWebServicesData(
            i,
            this.displayTreeHelper.nodeData(i).boundWebServicesIDs
        );
    }

    public selectNode(i:number, event:MouseEvent) {
        this.nodeInfoPopupService.setData(
            this.nodeInfoPopupDataCache.formNodeInfoPopupData(
                i,
                this.displayTreeHelper.nodeData(i),
                {x: event.clientX, y: event.clientY}
            )
        );
    }

    public deselectNode(event:MouseEvent) {
        this.nodeInfoPopupService.setData(null);
    }

    public get displayedNodesIndexes() {
        return this.nodesListIsEmpty === true
            ? [null]
            : (this.displayTreeHelper)
                ? this.displayTreeHelper.displayedNodesIndexes
                : null;
    }

    public isInitialized() {
        return this.initialized;
    }

    public isNoNodesLoaded() {
        return this.nodesListIsEmpty;
    }

    public fullTreeLayersCount(): number {
        return this.treeLayersCount;
    }

    public  maxDisplayedTreeLayers() {
        return this.maxDisplayedLayers
    }

    public node(index: number): NtwkNode {
        return this.displayTreeHelper.node(index);
    }

    public hasChildren(index: number): boolean {
        return this.displayTreeHelper.hasChildren(index);
    }

    public isCollapsed(index: number): boolean {
        return this.displayTreeHelper.isCollapsed(index);
    }

    public noChildrenChar(index: number): string {
        return this.displayTreeHelper.noChildrenChar(index);
    }

    public get isLoadingError() {
        return this.loadingError;
    }

    nodeTrackByFn(index:number, node_index:number) {
        if(!this.displayTreeHelper) return null;
        return this.displayTreeHelper.nodeData(node_index).node.id;
    }

    public foldBranch(index: number) {
        this.displayTreeHelper.foldBranch(index);
    }

    public unfoldBranch(index: number) {
        this.displayTreeHelper.unfoldBranch(index);
    }

    public changeVisibleLayers(range: Range) {
        this.startDisplayLayer = range.value;
        this.displayLayersCount = range.length;
        this.displayTreeHelper.setDisplayedTreeLayers(
            this.startDisplayLayer,
            this.displayLayersCount
        )
    }

    public refresh(_: boolean) {
        this.loadingError = false;
        this.initialize();
    }

    public pingBranch(i: number) {
        let branch = this.displayTreeHelper.flatPingTree[i];
        if(branch !== null) 
            this.pingCacheService.silentTreeUpdate(
                [branch]
            )
    }

    public isBranchPingable(i: number): boolean {
        if(this.treeLayersCount === 0) return false;
        let branch = this.displayTreeHelper.flatPingTree[i];
        return branch !== null && (
            branch.isBranchPingable || branch.isPingable
        );
    }

    constructor(
        private pingCacheService: PingCacheService,
        private nodeInfoPopupService: NodeInfoPopupDataService,
        private treeCollapsingService: TreeCollapsingService,
        private tagsService: TagsService,
        private nodesService: NodesService,
    ) {
        this.initialize();
    }

    private initialize() {
        this.nodesService.getNodesTree().subscribe(treeResult => {
            if(treeResult.success === false) {
                this.loadingError = true;
                return;
            }
            this.initialized = true;

            if(treeResult.data.nodesTree.allNodes.length === 0) {
                this.nodesListIsEmpty = true;
                return;
            } else this.nodesListIsEmpty = false;
            
            this.treeLayersCount = treeResult.data.nodesTree.treeLayers.length;
            this.displayLayersCount =
                this.treeLayersCount <= this.maxDisplayedLayers
                    ? this.treeLayersCount
                    : this.maxDisplayedLayers;
            this.displayTreeHelper = new DisplayTreeHelper(
                this.treeCollapsingService,
                treeResult.data.nodesTree.allNodes,
                treeResult.data.nodesTree.treeLayers,
                this.startDisplayLayer,
                this.displayLayersCount
            );
            this.nodeInfoPopupDataCache =
                new NodeInfoDataCache(
                    treeResult.data.nodesTree.allNodes.length,
                    treeResult.data.cwsData,
                    this.tagsService
                );
            this.loadingError = this.loadingError && this.nodeInfoPopupDataCache.loadingError;
        })
    }
}