import { NtwkNodesSubtree } from "../../model/viewModel/ntwkNodesSubtree.model";
import { PingTree } from "../../services/pingCache.service";


export class PingTreeBuilder {
    private static nodesSubtreeToPingTree(subtree: NtwkNodesSubtree) : PingTree  {
        let childrenPingTree: PingTree[] = [].concat(
            ...subtree.children.map(
                c => this.nodesSubtreeToPingTree(c)
            )
        );
        return {
            id: subtree.container.nodeData.node.id,
            isPingable: subtree.isPingable,
            isBranchPingable: subtree.isBranchPingable,
            childrenIDs: childrenPingTree
        };
    }

    private static flattenPingTrees(roots: PingTree[]): PingTree[] {
        let flatten = root => [root].concat(
            ...root.childrenIDs.map(flatten)
        )
        return [].concat(...roots.map(flatten))
    }

    private static cleanTreesFromUnpingableNodes(
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

    public static buildAndFlatten(fromSubtree: NtwkNodesSubtree[]): PingTree[] {
        let trees = fromSubtree
            .map( c => this.nodesSubtreeToPingTree(c));
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