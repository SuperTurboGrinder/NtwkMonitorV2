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
import { Range } from "../ui/misc/numRangeSelector.component";

@Component({
    selector: 'nodesTreeView',
    templateUrl: './nodesTreeView.component.html'
})
export class NodesTreeViewComponent implements OnDestroy {
    private displayTreeHelper: DisplayTreeHelper = null;
    private loadingError = false;
    private nodesTreeSubscription: Subscription = null;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;
    //private pingTreeBuilder: PingTreeBuilder = null;

    private readonly maxDisplayedLayers = 10;
    private startDisplayLayer = 0;
    private displayLayersCount = 0;
    private treeLayersCount = 0;
    //private nodesFlatPingTree: PingTree[] = null;

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
        return (this.displayTreeHelper)
            ? this.displayTreeHelper.displayedNodesIndexes
            : null;
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
        this.rebuildPingTree();
    }

    public unfoldBranch(index: number) {
        this.displayTreeHelper.unfoldBranch(index);
        this.rebuildPingTree();
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
        this.updateList();
    }

    private updateList() {
        this.loadingError = false;
        this.updateService.sendUpdateNodesAndTagsAlarm();
    }

    public pingBranch(i: number) {
        //let branch = this.nodesFlatPingTree[i];
        //if(branch !== null) 
        //    this.pingCacheService.silentTreeUpdate(
        //        [branch]
        //    )
    }

    public isBranchPingable(i: number): boolean {
        //if(this.treeLayersCount === 0) return false;
        //let branch = this.nodesFlatPingTree[i];
        //return branch !== null && (
        //    branch.isBranchPingable || branch.isPingable
        //);
        return true;
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
            this.displayLayersCount =
                this.treeLayersCount <= this.maxDisplayedLayers
                    ? this.treeLayersCount
                    : this.maxDisplayedLayers;
            //this.allLayers = treeResult.data.nodesTree.treeLayers;
            //this.containersIndexesInDepthLayers = treeResult.data.nodesTree.treeLayers
            //    .map(layer => layer.map(container => container.index));
            this.displayTreeHelper = new DisplayTreeHelper(
                treeCollapsingService,
                treeResult.data.nodesTree.allNodes,
                treeResult.data.nodesTree.treeLayers,
                this.startDisplayLayer,
                this.displayLayersCount
            );
            //this.pingTreeBuilder = new PingTreeBuilder(
            //    treeCollapsingService,
            //    treeResult.data.nodesTree.treeLayers
            //);
            //this.rebuildPingTree(
            //    this.startDisplayLayer,
            //    this.displayLayersCount
            //);
            this.renewPopupDataCache(
                new NodeInfoDataCache(
                    treeResult.data.nodesTree.allNodes.length,
                    treeResult.data.cwsData,
                    tagsService
                )
            );
            this.loadingError = this.loadingError && this.nodeInfoPopupDataCache.loadingError;
        })
    }

    private rebuildPingTree() {
        //this.nodesFlatPingTree = this.pingTreeBuilder.buildPingTree();
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

class NtwkNodesSubtree {
    constructor(
        public container: NtwkNodeDataContainer,
        public isBranchPingable: boolean,
        public children: NtwkNodesSubtree[]
    ) {}

    public get isPingable() : boolean {
        return this.container.nodeData.node.isOpenPing;
    }

    public static getFlatSubtree(
        subtree: NtwkNodesSubtree[]
    ) : NtwkNodesSubtree[] {
        if(subtree.length === 0)
            return subtree;
        let flattened: NtwkNodesSubtree[] = [];
        for(let i = 0; i< subtree.length; i++) {
            let subroot = subtree[i];
            flattened.push(subroot);
            flattened = flattened.concat(NtwkNodesSubtree
                .getFlatSubtree(subroot.children)
            );
        }
        return flattened;
    }
}

class TreeTrimmingHelper {
    private lastBottomLayer = -1;
    private lastDepth = -1;
    private lastSubtree: NtwkNodesSubtree[] = null;

    constructor(
        private wholeTreeLayers: NtwkNodeDataContainer[][],
        private treeCollapsingService: TreeCollapsingService
    ) {}

    public get fullTreeHeight(): number {
        return this.wholeTreeLayers.length;
    }

    public getSubtree(
        fromLayer: number, depth: number
    ): NtwkNodesSubtree[] {
        let result = fromLayer === this.lastBottomLayer
            && depth === this.lastDepth
                ? this.lastSubtree
                : this.setLastSubtree(
                    this.wholeTreeLayers[fromLayer].map(
                        container => this.nodeContainerIntoSubtree(
                            container,
                            fromLayer+depth-1
                        )
                    )
                );
        this.lastBottomLayer = fromLayer;
        this.lastDepth = depth;
        return result;
    }

    private setLastSubtree(
        subtree: NtwkNodesSubtree[]
    ) : NtwkNodesSubtree[] {
        this.lastSubtree = subtree;
        return subtree;
    }

    private nodeContainerIntoSubtree(
        c: NtwkNodeDataContainer,
        maxDepth: number
    ) : NtwkNodesSubtree {
        let children: NtwkNodesSubtree[] = c.depth === maxDepth
        || this.treeCollapsingService
        .isCollapsed(c.nodeData.node.id)
            ? []
            : c.children
                .map(c => this.nodeContainerIntoSubtree(c, maxDepth));
        let isBranchPingable: boolean = children.some(
            c => c.isBranchPingable
            || c.container.nodeData.node.isOpenPing
        )
        return new NtwkNodesSubtree(c, isBranchPingable, children);
    }
}

class DisplayTreeHelper {
    //private flatListIndexesByLayer: number[][] = null;
    private flattenedTreeOfDisplayedNodesIndexes: number[] = null;
    private displayedNodesPrefixes: string[] = null;
    private treeTrimmingHelper: TreeTrimmingHelper = null;

    constructor(
        private treeCollapsingService: TreeCollapsingService,
        private flatNodesList: NtwkNodeDataContainer[],
        nodesTreeLayers: NtwkNodeDataContainer[][],
        private startLayerNumber: number,
        private displayedLayersCount: number
    ) {
        this.treeTrimmingHelper = new TreeTrimmingHelper(
            nodesTreeLayers, treeCollapsingService
        );
        this.buildInternalStructures(
            startLayerNumber,
            displayedLayersCount
        );
    }

    public get displayedNodesIndexes() {
        return this.flattenedTreeOfDisplayedNodesIndexes;
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

    public prefix(prefixIndex: number): string {
        return this.displayedNodesPrefixes[prefixIndex];
    }

    public setDisplayedTreeLayers(
        startLayerNumber: number,
        displayedLayersCount: number
    ) {
        this.startLayerNumber = startLayerNumber;
        this.displayedLayersCount = displayedLayersCount;
        this.rebuildDisplayedList();
    }

    public foldBranch(i: number) {
        this.treeCollapsingService.foldSubtree(
            this.node(i).id
        )
        this.rebuildDisplayedList();
    }

    public unfoldBranch(i: number) {
        this.treeCollapsingService.unfoldSubtree(
            this.node(i).id
        )
        this.rebuildDisplayedList();
    }

    private rebuildDisplayedList() {
        let subtree = this.treeTrimmingHelper
            .getSubtree(this.startLayerNumber, this.displayedLayersCount);
        this.flattenedTreeOfDisplayedNodesIndexes = NtwkNodesSubtree
            .getFlatSubtree(subtree).map(n => n.container.index);
        this.buildPrefixes(this.flattenedTreeOfDisplayedNodesIndexes);
    }

    private buildInternalStructures(
        startLayerNumber: number,
        displayedLayersCount: number
    ) {
        this.setDisplayedTreeLayers(
            startLayerNumber, 
            displayedLayersCount
        );
    }

    private containerOfDisplayedNode(indexIntoDisplayedNodesIndexes: number) {
        return this.flatNodesList[
            this.flattenedTreeOfDisplayedNodesIndexes[
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
        let prefixesNum = this.flattenedTreeOfDisplayedNodesIndexes.length;
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
        this.displayedNodesPrefixes = prefixes.slice(1, prefixes.length);
    }
}

class PingTreeBuilder {
    constructor(
        private treeCollapsingService: TreeCollapsingService,
        private treeLayers: NtwkNodeDataContainer[][]
    ) {}

    private nodeDataContainerToPingTree(
        container: NtwkNodeDataContainer,
        maxDepth: number
    ) : PingTree  {
        let isSubtreeCollapsed = this.treeCollapsingService
            .isCollapsed(container.nodeData.node.id)
        let childrenPingTree: PingTree[] = isSubtreeCollapsed
        ||  container.depth === maxDepth
            ? []
            : [].concat(...container.children.map(
                c => this.nodeDataContainerToPingTree(c, maxDepth)
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

    /*public buildPingTree(fromLayer: number, layersCount: number): PingTree[] {
        let trees = this.treeLayers[fromLayer]
            .map( c => this.nodeDataContainerToPingTree(c));
        let result = this.flattenPingTrees(trees, layersCount)
            .map(root => 
                root.isPingable === false
                && root.isBranchPingable === false
                    ? null
                    : root
            );
        trees = this.cleanTreesFromUnpingableNodes(trees);
        return result;
    }*/
}