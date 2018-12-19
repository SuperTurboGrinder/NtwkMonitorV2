import { NtwkNodeDataContainer } from './ntwkNodeDataContainer.model';

export class NtwkNodesSubtree {
    constructor(
        public container: NtwkNodeDataContainer,
        public isBranchPingable: boolean,
        public children: NtwkNodesSubtree[]
    ) {}

    public get isPingable(): boolean {
        return this.container.nodeData.node.isOpenPing;
    }

    public static getFlatSubtree(
        subtree: NtwkNodesSubtree[]
    ): NtwkNodesSubtree[] {
        if (subtree.length === 0) {
            return subtree;
        }
        const flattened: NtwkNodesSubtree[] = [];
        for (const st of subtree) {
            NtwkNodesSubtree.flatten(flattened, st);
        }
        return flattened;
    }

    private static flatten(
        acc: NtwkNodesSubtree[],
        subtree: NtwkNodesSubtree
    ) {
        acc.push(subtree);
        for (const st of subtree.children) {
            NtwkNodesSubtree.flatten(acc, st);
        }
    }
}
