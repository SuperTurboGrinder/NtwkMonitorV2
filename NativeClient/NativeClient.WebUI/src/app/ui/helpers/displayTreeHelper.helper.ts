import { TreeTrimmingHelper } from './treeTrimmingHelper.helper';
import { TreeCollapsingService } from '../../services/treeCollapsing.service';
import { NtwkNodeDataContainer } from '../../model/viewModel/ntwkNodeDataContainer.model';
import { NtwkNode } from '../../model/httpModel/ntwkNode.model';
import { NodeData } from '../../model/httpModel/nodeData.model';
import { NtwkNodesSubtree } from '../../model/viewModel/ntwkNodesSubtree.model';
import { PingTree } from '../../services/massPing.service';
import { PingTreeBuilder } from './pingTreeBuilder.helper';

export class DisplayTreeHelper {
    private flattenedTreeOfDisplayedNodesIndexes: number[] = null;
    private displayedNodesPrefixes: string[] = null;
    private treeTrimmingHelper: TreeTrimmingHelper = null;
    private flattenedPingTree: PingTree[] = null;

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
            .children.length === 0 ? '─' : '';
    }

    public hasChildren(flatNodesListIndex: number): boolean {
        return this.flatNodesList[flatNodesListIndex]
            .children.length > 0;
    }

    public node(index: number): NtwkNode {
        return this.flatNodesList[index].nodeData.node;
    }

    public nodeData(index: number): NodeData {
        return this.flatNodesList[index].nodeData;
    }

    public isCollapsed(index: number): boolean {
        return this.treeCollapsingService === null
            ? false
            : this.treeCollapsingService.isCollapsed(
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
        if (this.treeCollapsingService !== null) {
            this.treeCollapsingService.foldSubtree(
                this.node(i).id
            );
            this.rebuildDisplayedList(true);
        }
    }

    public unfoldBranch(i: number) {
        if (this.treeCollapsingService !== null) {
            this.treeCollapsingService.unfoldSubtree(
                this.node(i).id
            );
            this.rebuildDisplayedList(true);
        }
    }

    public get flatPingTree(): PingTree[] {
        return this.flattenedPingTree;
    }

    private rebuildDisplayedList(forcedUpdate: boolean = false) {
        const subtree = this.treeTrimmingHelper
            .getSubtree(
                this.startLayerNumber,
                this.displayedLayersCount,
                forcedUpdate
            );
        this.flattenedTreeOfDisplayedNodesIndexes = NtwkNodesSubtree
            .getFlatSubtree(subtree).map(n => n.container.index);
        this.buildPrefixes(this.flattenedTreeOfDisplayedNodesIndexes);
        this.flattenedPingTree = PingTreeBuilder.buildAndFlatten(subtree);
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
        if (!displayedNodesIndexes || displayedNodesIndexes.length === 0) {
            return;
        }
        const branchLengthStack: number[] = [];
        let previousLayer = this.containerOfDisplayedNode(0).depth;
        const startLayer = previousLayer;
        const lastOn = arr => arr[arr.length - 1];
        const decrementLastValue = arr => arr[arr.length - 1]--;
        const prefixesNum = this.flattenedTreeOfDisplayedNodesIndexes.length;
        let prefixes: string[] = [''];
        let previousNodeContainer: NtwkNodeDataContainer = null;
        for (let i = 0; i < prefixesNum; i++) {
            const currentNodeContainer = this.containerOfDisplayedNode(i);
            const currentLayer = currentNodeContainer.depth;
            if (currentLayer === startLayer) {
                prefixes.push(prefixes[0]);
            } else {
                let prefix = lastOn(prefixes);
                if (previousLayer === currentLayer) {
                    if (lastOn(branchLengthStack) === 1) {
                        prefix = prefix.substr(0, prefix.length - 1) + '└';
                    }
                    if (branchLengthStack.length > 0) {
                        decrementLastValue(branchLengthStack);
                        if (lastOn(branchLengthStack) === 0) {
                            branchLengthStack.pop();
                        }
                    }
                } else if (currentLayer > previousLayer) {
                    // push to stack, add to prefix
                    prefix = prefix.length === 0 ? prefix
                        : lastOn(prefix) === '└'
                            ? prefix.substr(0, prefix.length - 1) + ' '
                            : lastOn(prefix) === '├'
                                ? prefix.substr(0, prefix.length - 1) + '│'
                                : prefix;
                    const branchLength = previousNodeContainer.children.length;
                    if (branchLength > 1) {
                        branchLengthStack.push(branchLength - 1);
                    }
                    prefix = prefix + ((branchLength === 1) ? '└' : '├');
                } else { // current < prev
                    // use last on stack or empty
                    prefix = branchLengthStack.length === 0 ? prefixes[0]
                        : prefix.substr(0, currentLayer - startLayer - 1)
                            +  ((lastOn(branchLengthStack) === 1) ? '└' : '├');
                    if (branchLengthStack.length > 0) {
                        decrementLastValue(branchLengthStack);
                        if (lastOn(branchLengthStack) === 0) {
                            branchLengthStack.pop();
                        }
                    }
                }
                prefixes.push(prefix);
            }
            previousNodeContainer = currentNodeContainer;
            previousLayer = currentLayer;
        }
        prefixes = prefixes.slice(1, prefixes.length);
        if (this.treeCollapsingService === null) {
            prefixes = prefixes.map((p, i) => p + (
                this.containerOfDisplayedNode(i).children.length === 0
                    ? '─' : '┬'
            ));
        }
        this.displayedNodesPrefixes = prefixes;
    }
}
