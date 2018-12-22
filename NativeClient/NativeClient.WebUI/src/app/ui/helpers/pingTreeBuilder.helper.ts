import { NtwkNodesSubtree } from '../../model/viewModel/ntwkNodesSubtree.model';
import { PingTree } from 'src/app/model/servicesModel/pingTree.model';
import { ObjectTreeAlg } from 'src/app/model/viewModel/baseObjectTree';
import * as _ from 'lodash';


export class PingTreeBuilder {
    private static nodesSubtreeToPingTree(subtree: NtwkNodesSubtree): PingTree  {
        return {
            id: subtree.container.nodeData.node.id,
            name: subtree.container.nodeData.node.name,
            isPingable: subtree.isPingable,
            isBranchPingable: subtree.isBranchPingable,
            children: subtree.children.map(
                c => this.nodesSubtreeToPingTree(c)
            )
        };
    }

    private static cleanTreesFromUnpingableNodes(
        roots: PingTree[]
    ): PingTree[] {
        const result = roots.filter(root =>
            root.isPingable === false
            && root.isBranchPingable === false
        );
        for (const root of result) {
            root.children = this.cleanTreesFromUnpingableNodes(
                root.children
            );
        }
        return result;
    }

    public static buildAndFlatten(fromSubtree: NtwkNodesSubtree[]): PingTree[] {
        let trees = fromSubtree
            .map( c => this.nodesSubtreeToPingTree(c));
        const result = ObjectTreeAlg.getFlatSubtree(trees)
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
