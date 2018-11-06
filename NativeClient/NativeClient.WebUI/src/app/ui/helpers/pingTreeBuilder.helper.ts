import { NtwkNodesSubtree } from '../../model/viewModel/ntwkNodesSubtree.model';
import { PingTree } from '../../services/pingCache.service';


export class PingTreeBuilder {
    private static nodesSubtreeToPingTree(subtree: NtwkNodesSubtree): PingTree  {
        return {
            id: subtree.container.nodeData.node.id,
            isPingable: subtree.isPingable,
            isBranchPingable: subtree.isBranchPingable,
            children: subtree.children.map(
                c => this.nodesSubtreeToPingTree(c)
            )
        };
    }

    private static flattenPingTrees(roots: PingTree[]): PingTree[] {
        const flatten = (root: PingTree) => [root].concat(
            ...root.children.map(flatten)
        );
        return [].concat(...roots.map(flatten));
    }

    private static cleanTreesFromUnpingableNodes(
        roots: PingTree[]
    ): PingTree[] {
        const result = roots.map(root =>
            root.isPingable === false
            && root.isBranchPingable === false
                ? null : root
        ).filter(root => root != null);
        result.forEach(root =>
            root.children = this.cleanTreesFromUnpingableNodes(
                root.children
            )
        );
        return result;
    }

    public static buildAndFlatten(fromSubtree: NtwkNodesSubtree[]): PingTree[] {
        let trees = fromSubtree
            .map( c => this.nodesSubtreeToPingTree(c));
        const result = this.flattenPingTrees(trees)
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
