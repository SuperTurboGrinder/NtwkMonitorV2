import { NtwkNodesSubtree } from "../../model/viewModel/ntwkNodesSubtree.model"
import { NtwkNodeDataContainer } from "../../model/viewModel/ntwkNodeDataContainer.model";
import { TreeCollapsingService } from "../../services/treeCollapsing.service";

export class TreeTrimmingHelper {
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
        fromLayer: number, depth: number, forceUpdate: boolean = false
    ): NtwkNodesSubtree[] {
        let result = fromLayer === this.lastBottomLayer
            && depth === this.lastDepth && !forceUpdate
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
