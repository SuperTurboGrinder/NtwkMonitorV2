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
    private allNodesList: NtwkNodeDataContainer[] = null;
    private rootLayer: NtwkNodeDataContainer[] = null;
    private layers: number[][];
    private loadingError = false;
    private nodesTreeSubscription: Subscription = null;
    private nodeInfoPopupDataCache: NodeInfoDataCache = null;

    private startDisplayLayer = 0;
    private displayLayersCount = 10;
    private displayedNodesIndexes: number[] = null;
    private displayedNodesPrefixes: string[] = null;
    private displayedNodesFlatPingTree: PingTree[] = null;

    public prefix(prefixIndex: number) {
        return this.displayedNodesPrefixes[prefixIndex];
    }

    private rebuildDisplayedList() {
        let flattenSubtree = (indexes:number[], depth:number) => {
            if(depth === 0 || indexes.length === 0) return indexes;
            let next: number[] = [];
            for(let i = 0; i< indexes.length; i++) {
                next.push(indexes[i]);
                let nodeContainer = this.allNodesList[indexes[i]];
                next = this.treeCollapsingService.isCollapsed(
                    nodeContainer.nodeData.node.id
                ) || nodeContainer.children.length === 0
                    ? next
                    : next.concat(flattenSubtree(
                        nodeContainer.children.map(c => c.index),
                        depth-1
                    ))
            }
            return next;
        }
        let startLayer = this.layers[this.startDisplayLayer];
        this.displayedNodesIndexes = flattenSubtree(
            startLayer,
            this.displayLayersCount-1
        );
        this.displayedNodesFlatPingTree = this.buildPingTree(
            this.rootLayer
        );
        this.buildPrefixes();
    }
    
    buildPrefixes() {
        if(!this.displayedNodesIndexes || this.displayedNodesIndexes.length == 0)
            return;
        let branchLengthStack: number[] = [];
        let containerOf = indexIndex => this.allNodesList[this.displayedNodesIndexes[indexIndex]];
        let previousLayer = containerOf(0).depth;
        let startLayer = previousLayer;
        let last = arr => arr[arr.length-1];
        let decrLast = arr => arr[arr.length-1]--;
        let prefixesNum = this.displayedNodesIndexes.length;
        let prefixes: string[] = [""];
        let previousNodeContainer: NtwkNodeDataContainer = null;
        for(let i = 0; i < prefixesNum; i++) {
            let currentNodeContainer = containerOf(i);
            let currentLayer = currentNodeContainer.depth;
            if(currentLayer == startLayer) {
                prefixes.push(prefixes[0]);
            }
            else {
                let prefix = last(prefixes);
                if(previousLayer == currentLayer) {
                    if(last(branchLengthStack) == 1) {
                        prefix = prefix.substr(0, prefix.length-1)+"└"
                    }
                    if(branchLengthStack.length > 0) {
                        decrLast(branchLengthStack);
                        if(last(branchLengthStack) == 0) {
                            branchLengthStack.pop();
                        }
                    }
                } else if(currentLayer > previousLayer) {
                    prefix = prefix.length == 0 ? prefix
                        : last(prefix) == "└"
                            ? prefix.substr(0, prefix.length-1)+" "
                            : last(prefix) == "├"
                                ? prefix.substr(0, prefix.length-1)+ "│"
                                : prefix;
                    let branchLength = previousNodeContainer.children.length;
                    if(branchLength > 1) branchLengthStack.push(branchLength-1);
                    prefix = prefix + ((branchLength == 1) ? "└" : "├")
                    //push to stack add to prefix
                } else { //current < prev
                    prefix = branchLengthStack.length == 0 ? prefixes[0]
                        : prefix.substr(0, currentLayer - startLayer)
                            +  (last(branchLengthStack) == 1) ? "└" : "├";
                    if(branchLengthStack.length > 0) {
                        decrLast(branchLengthStack);
                        if(last(branchLengthStack) == 0) {
                            branchLengthStack.pop();
                        }
                    }
                    //use last on stack or empty
                }
                prefixes.push(prefix);
            }
            previousNodeContainer = currentNodeContainer;
            previousLayer = currentLayer;
        }
        this.displayedNodesPrefixes = prefixes.slice(1, prefixes.length-1);
    }

    noChildrenChar(i: number): string {
        return this.allNodesList[i].children.length == 0 ? "─" : "";
    }

    hasChildren(i: number) : boolean {
        return this.allNodesList[i].children.length > 0
    }

    node(index:number) : NtwkNode {
        return this.allNodesList[index].nodeData.node;
    }

    nodeData(index:number) : NodeData {
        return this.allNodesList[index].nodeData;
    }

    isCollapsed(index: number): boolean {
        return this.treeCollapsingService.isCollapsed(
            this.node(index).id
        );
    }

    foldBranch(i: number) {
        this.treeCollapsingService.foldSubtree(
            this.node(i).id
        )
        this.rebuildDisplayedList();
    }

    unfoldBranch(i: number) {
        this.treeCollapsingService.unfoldSubtree(
            this.node(i).id
        )
        this.rebuildDisplayedList();
    }
    
    listWebServicesData(i:number) : {name:string, id:number}[] {
        return this.nodeInfoPopupDataCache.listWebServicesData(
            i,
            this.nodeData(i).boundWebServicesIDs
        );
    }

    public selectNode(i:number, event:MouseEvent) {
        this.nodeInfoPopupService.setData(
            this.nodeInfoPopupDataCache.formNodeInfoPopupData(
                i,
                this.nodeData(i),
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
        if(!this.allNodesList) return null;
        return this.nodeData(node_index).node.id;
    }

    public refresh(_: boolean) {
        this.updateList();
    }

    private updateList() {
        this.loadingError = false;
        this.updateService.sendUpdateNodesAndTagsAlarm();
    }

    public pingBranch(i: number) {
        this.pingCacheService.silentTreeUpdate(
            [this.displayedNodesFlatPingTree[i]]
        )
    }

    constructor(
        private updateService: UpdateAlarmService,
        private pingCacheService: PingCacheService,
        private nodeInfoPopupService: NodeInfoPopupDataService,
        private treeCollapsingService: TreeCollapsingService,
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
            
            this.allNodesList = treeResult.data.nodesTree.allNodes;
            this.rootLayer = treeResult.data.nodesTree.treeLayers[0];
            this.layers = treeResult.data.nodesTree.treeLayers
                .map(layer => layer.map(container => container.index));
            this.rebuildDisplayedList();
            this.renewPopupDataCache(
                new NodeInfoDataCache(
                    this.allNodesList.length,
                    treeResult.data.cwsData,
                    tagsService
                )
            );
            this.loadingError = this.loadingError && this.nodeInfoPopupDataCache.loadingError;
        })
    }

    private buildPingTree(firstLayer: NtwkNodeDataContainer[]): PingTree[] {
        let toPingTree =
        (container: NtwkNodeDataContainer) : PingTree => {
            let childrenPingTree: PingTree[] = this.treeCollapsingService.isCollapsed(
                container.nodeData.node.id
            ) ? [] : [].concat(...container.children.map(
                c => toPingTree(c)
            ));
            return {
                id: container.nodeData.node.id,
                childrenIDs: childrenPingTree
            };
        }
        let flattenPingTree = (roots: PingTree[]): PingTree[] => {
            let flatten = root => [root].concat(
                ...root.childrenIDs.map(flatten)
            )
            return [].concat(...roots.map(flatten))
        }
        return flattenPingTree(firstLayer.map(toPingTree));
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