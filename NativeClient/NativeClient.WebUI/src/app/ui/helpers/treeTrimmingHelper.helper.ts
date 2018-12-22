import { NtwkNodesSubtree } from '../../model/viewModel/ntwkNodesSubtree.model';
import { NtwkNodeDataContainer } from '../../model/viewModel/ntwkNodeDataContainer.model';
import { TreeCollapsingService } from '../../services/treeCollapsing.service';
import * as _ from 'lodash';

export class TreeTrimmingHelper {
    private lastBottomLayer = -1;
    private lastDepth = -1;
    private lastSubtree: NtwkNodesSubtree[] = null;

    private static sortContainersByNodePort(
        containers: NtwkNodeDataContainer[]
    ): _.LoDashExplicitWrapper<NtwkNodeDataContainer[]> {
        if (containers === []) {
            return _.chain(containers);
        } else {
            return _.chain(containers)
                .filter(c => c.nodeData.node.parentPort !== null)
                .sort((c1, c2) => c1.nodeData.node.parentPort - c2.nodeData.node.parentPort)
                .concat(
                    containers.filter(c => c.nodeData.node.parentPort === null)
                );
        }
    }

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
        const result = fromLayer === this.lastBottomLayer
            && depth === this.lastDepth && !forceUpdate
                ? this.lastSubtree
                : this.setLastSubtree(
                    this.wholeTreeLayers[fromLayer].map(
                        container => this.nodeContainerIntoSubtree(
                            container,
                            fromLayer + depth - 1
                        )
                    )
                );
        this.lastBottomLayer = fromLayer;
        this.lastDepth = depth;
        return result;
    }

    private setLastSubtree(
        subtree: NtwkNodesSubtree[]
    ): NtwkNodesSubtree[] {
        this.lastSubtree = subtree;
        return subtree;
    }

    private nodeContainerIntoSubtree(
        cont: NtwkNodeDataContainer,
        maxDepth: number
    ): NtwkNodesSubtree {
        const children: NtwkNodesSubtree[] = cont.depth === maxDepth
        || (this.treeCollapsingService !== null
            && this.treeCollapsingService.isCollapsed(cont.nodeData.node.id))
                ? []
                : TreeTrimmingHelper
                    .sortContainersByNodePort(cont.children)
                        .map(
                            c => this.nodeContainerIntoSubtree(c, maxDepth)
                        ).value();
        const isBranchPingable: boolean = children.some(
            c => c.isBranchPingable
            || c.container.nodeData.node.isOpenPing
        );
        return new NtwkNodesSubtree(cont, isBranchPingable, children);
    }
}
