import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";

import { NtwkNode } from "../model/httpModel/ntwkNode.model";
import { CWSData } from "../model/httpModel/cwsData.model";
import { NodesService } from "../services/nodes.service";
import { NodeData } from "../model/httpModel/nodeData.model";
import { SettingsProfilesService } from "../services/settingsProfiles.service";
import { UpdateAlarmService } from "../services/updateAlarm.service";
import { PingCacheService, PingTree } from "../services/pingCache.service";
import { NodeTag } from "../model/httpModel/nodeTag.model";
import { TagsService } from "../services/tags.service";
import { NodeInfoPopupDataService } from "../services/nodeInfoPopupData.service";
import { NodeInfoPopupData } from "../model/viewModel/nodeInfoPopupData.model";
import { NtwkNodesTree } from "../model/viewModel/ntwkNodesTree.model";
import { NtwkNodeDataContainer } from "../model/viewModel/ntwkNodeDataContainer.model";
import { NodeInfoDataCache } from "./helpers/nodesInfoDataCache.helper";
import { TreeCollapsingService } from "../services/treeCollapsing.service";
import { PingService } from "../services/ping.service";

@Component({
    selector: 'nodesTreeView',
    templateUrl: './nodesTreeView.component.html'
})
export class NodesTreeViewComponent implements OnDestroy {
    private displayTreeHelper: DisplayTreeHelper = null;
    //private allNodesList: NtwkNodeDataContainer[] = null;
    //private allLayers: NtwkNodeDataContainer[][] = null;
    //private containersIndexesInDepthLayers: number[][];
    private loadingError = false;
    private nodesTreeSubscription: Subscription = null;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;

    private startDisplayLayer = 0;
    private displayLayersCount = 10;
    private treeLayersCount = 0;
    //private displayedNodesIndexes: number[] = null;
    //private displayedNodesPrefixes: string[] = null;
    private displayedNodesFlatPingTree: PingTree[] = null;

    public prefix(prefixIndex: number) {
        //return this.displayedNodesPrefixes[prefixIndex];
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

    public get isLoadingError() {
        return this.loadingError;
    }

    nodeTrackByFn(index:number, node_index:number) {
        if(!this.displayTreeHelper) return null;
        return this.displayTreeHelper.nodeData(node_index).node.id;
    }

    public refresh(_: boolean) {
        this.updateList();
    }

    private updateList() {
        this.loadingError = false;
        this.updateService.sendUpdateNodesAndTagsAlarm();
    }

    public pingBranch(i: number) {
        let branch = this.displayedNodesFlatPingTree[i];
        if(branch !== null) 
            this.pingCacheService.silentTreeUpdate(
                [branch]
            )
    }

    public isBranchPingable(i: number) {
        let branch = this.displayedNodesFlatPingTree[i];
        return branch !== null && (
            branch.isBranchPingable || branch.isPingable
        );
    }

    constructor(
        private updateService: UpdateAlarmService,
        private pingCacheService: PingCacheService,
        private nodeInfoPopupService: NodeInfoPopupDataService,
        treeCollapsingService: TreeCollapsingService,
        tagsService: TagsService,
        nodesService: NodesService,
    ) {
        this.nodesTreeSubscription = nodesService.getNodesTree().subscribe(treeResult => {
            if(treeResult === null) {
                return;
            } else if(treeResult.success === false) {
                this.loadingError = true;
                return;
            }
            
            //this.allNodesList = treeResult.data.nodesTree.allNodes;
            this.treeLayersCount = treeResult.data.nodesTree.treeLayers.length;
            //this.allLayers = treeResult.data.nodesTree.treeLayers;
            //this.containersIndexesInDepthLayers = treeResult.data.nodesTree.treeLayers
            //    .map(layer => layer.map(container => container.index));
            console.log("TEST1");
            this.displayTreeHelper = new DisplayTreeHelper(
                treeCollapsingService,
                treeResult.data.nodesTree.allNodes,
                treeResult.data.nodesTree.treeLayers,
                0, 10
            );
            console.log("TEST2");
            this.renewPopupDataCache(
                new NodeInfoDataCache(
                    treeResult.data.nodesTree.allNodes.length,
                    treeResult.data.cwsData,
                    tagsService
                )
            );
            console.log("TEST3")
            this.loadingError = this.loadingError && this.nodeInfoPopupDataCache.loadingError;
        })
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

class DisplayTreeHelper {
    private flatListIndexesByLayer: number[][] = null;
    private flattenedListOfDisplayedNodesIndexes: number[] = null;
    private displayedNodesPrefixes: string[] = null;

    constructor(
        private treeCollapsingService: TreeCollapsingService,
        private flatNodesList: NtwkNodeDataContainer[],
        private nodesTreeLayers: NtwkNodeDataContainer[][],
        private startLayerNumber: number,
        private displayedLayersCount: number
    ) {
        this.buildInternalStructures(
            startLayerNumber,
            displayedLayersCount
        );
    }

    public noChildrenChar(flatNodesListIndex: number): string {
        return this.flatNodesList[flatNodesListIndex]
            .children.length == 0 ? "─" : "";
    }

    public hasChildren(flatNodesListIndex: number) : boolean {
        return this.flatNodesList[flatNodesListIndex]
            .children.length > 0
    }

    public node(index:number) : NtwkNode {
        return this.flatNodesList[index].nodeData.node;
    }

    public nodeData(index:number) : NodeData {
        return this.flatNodesList[index].nodeData;
    }

    public isCollapsed(index: number): boolean {
        return this.treeCollapsingService.isCollapsed(
            this.node(index).id
        );
    }

    public setDisplayedTreeLayers(
        startLayerNumber: number,
        displayedLayersCount: number
    ) {
        this.startLayerNumber = startLayerNumber;
        this.displayedLayersCount = displayedLayersCount;
        this.rebuildDisplayedList(startLayerNumber, displayedLayersCount);
    }

    public foldBranch(i: number) {
        this.treeCollapsingService.foldSubtree(
            this.node(i).id
        )
        this.rebuildDisplayedList(
            this.startLayerNumber,
            this.displayedLayersCount
        );
    }

    public unfoldBranch(i: number) {
        this.treeCollapsingService.unfoldSubtree(
            this.node(i).id
        )
        this.rebuildDisplayedList(
            this.startLayerNumber,
            this.displayedLayersCount
        );
    }

    private rebuildDisplayedList(
        startLayerNumber: number,
        displayedLayersCount: number
    ) {
        let startLayerIndexes
            = this.flatListIndexesByLayer[startLayerNumber];
        this.flattenedListOfDisplayedNodesIndexes = this.flattenSubtrees(
            startLayerIndexes,
            displayedLayersCount-1
        );
        this.buildPrefixes(this.flattenedListOfDisplayedNodesIndexes);
    }

    private buildInternalStructures(
        startLayerNumber: number,
        displayedLayersCount: number
    ) {
        this.flatListIndexesByLayer = this.nodesTreeLayers
            .map(layer => layer.map(container => container.index));
        this.rebuildDisplayedList(startLayerNumber, displayedLayersCount);
    }

    private flattenSubtrees(
        flatListRootsIndexes:number[],
        layersCount:number
    ) {
        if(layersCount === 0 || flatListRootsIndexes.length === 0)
            return flatListRootsIndexes;
        let flattened: number[] = [];
        for(let i = 0; i< flatListRootsIndexes.length; i++) {
            let subtreeRootIndex = flatListRootsIndexes[i];
            flattened.push(subtreeRootIndex);
            let nodeContainer = this.flatNodesList[subtreeRootIndex];
            let isSubtreeCollapsed = this.treeCollapsingService
                .isCollapsed(nodeContainer.nodeData.node.id)
            flattened = isSubtreeCollapsed
                || nodeContainer.children.length === 0
                ? flattened
                : flattened.concat(this.flattenSubtrees(
                    nodeContainer.children.map(c => c.index),
                    layersCount-1
                ))
        }
        return flattened;
    }

    private containerOfDisplayedNode(indexIntoDisplayedNodesIndexes: number) {
        return this.flatNodesList[
            this.flattenedListOfDisplayedNodesIndexes[
                indexIntoDisplayedNodesIndexes
            ]
        ];
    }

    private buildPrefixes(displayedNodesIndexes: number[]) {
        if(!displayedNodesIndexes || displayedNodesIndexes.length == 0)
            return;
        let branchLengthStack: number[] = [];
        let previousLayer = this.containerOfDisplayedNode(0).depth;
        let startLayer = previousLayer;
        let lastOn = arr => arr[arr.length-1];
        let decrementLastValue = arr => arr[arr.length-1]--;
        let prefixesNum = this.flattenedListOfDisplayedNodesIndexes.length;
        let prefixes: string[] = [""];
        let previousNodeContainer: NtwkNodeDataContainer = null;
        for(let i = 0; i < prefixesNum; i++) {
            let currentNodeContainer = this.containerOfDisplayedNode(i);
            let currentLayer = currentNodeContainer.depth;
            if(currentLayer == startLayer) {
                prefixes.push(prefixes[0]);
            }
            else {
                let prefix = lastOn(prefixes);
                if(previousLayer == currentLayer) {
                    if(lastOn(branchLengthStack) == 1) {
                        prefix = prefix.substr(0, prefix.length-1)+"└"
                    }
                    if(branchLengthStack.length > 0) {
                        decrementLastValue(branchLengthStack);
                        if(lastOn(branchLengthStack) == 0) {
                            branchLengthStack.pop();
                        }
                    }
                } else if(currentLayer > previousLayer) {
                    //push to stack, add to prefix
                    prefix = prefix.length == 0 ? prefix
                        : lastOn(prefix) == "└"
                            ? prefix.substr(0, prefix.length-1)+" "
                            : lastOn(prefix) == "├"
                                ? prefix.substr(0, prefix.length-1)+ "│"
                                : prefix;
                    let branchLength = previousNodeContainer.children.length;
                    if(branchLength > 1) branchLengthStack.push(branchLength-1);
                    prefix = prefix + ((branchLength == 1) ? "└" : "├")
                } else { //current < prev
                    //use last on stack or empty
                    prefix = branchLengthStack.length == 0 ? prefixes[0]
                        : prefix.substr(0, currentLayer - startLayer)
                            +  (lastOn(branchLengthStack) == 1) ? "└" : "├";
                    if(branchLengthStack.length > 0) {
                        decrementLastValue(branchLengthStack);
                        if(lastOn(branchLengthStack) == 0) {
                            branchLengthStack.pop();
                        }
                    }
                }
                prefixes.push(prefix);
            }
            previousNodeContainer = currentNodeContainer;
            previousLayer = currentLayer;
        }
        this.displayedNodesPrefixes = prefixes.slice(1, prefixes.length-1);
    }
}

class PingTreeBuilder {
    constructor(
        private treeCollapsingService: TreeCollapsingService
    ) {}

    private nodeDataContainerToPingTree(
        container: NtwkNodeDataContainer
    ) : PingTree  {
        let isSubtreeCollapsed = this.treeCollapsingService
            .isCollapsed(container.nodeData.node.id)
        let childrenPingTree: PingTree[] = isSubtreeCollapsed
            ? []
            : [].concat(...container.children.map(
                c => this.nodeDataContainerToPingTree(c)
            ));
        return {
            id: container.nodeData.node.id,
            isPingable: container.nodeData.node.isOpenPing,
            isBranchPingable: childrenPingTree.some(
                child => child.isPingable || child.isBranchPingable
            ),
            childrenIDs: childrenPingTree
        };
    }

    private flattenPingTrees(roots: PingTree[]): PingTree[] {
        let flatten = root => [root].concat(
            ...root.childrenIDs.map(flatten)
        )
        return [].concat(...roots.map(flatten))
    }

    private cleanTreesFromUnpingableNodes(
        roots: PingTree[]
    ): PingTree[] {
        let result = roots.map(root =>
            root.isPingable === false
            && root.isBranchPingable === false
                ? null : root
        ).filter(root => root != null);
        result.forEach(root =>
            root.childrenIDs = this.cleanTreesFromUnpingableNodes(
                root.childrenIDs
            )
        )
        return result;
    }

    public buildPingTree(
        firstLayer: NtwkNodeDataContainer[]
    ): PingTree[] {
        let trees = firstLayer.map(this.nodeDataContainerToPingTree);
        let result = this.flattenPingTrees(trees)
            .map(root => 
                root.isPingable === false
                && root.isBranchPingable === false
                    ? null
                    : root
            );
        trees = this.cleanTreesFromUnpingableNodes(trees);
        return result;
    }
}