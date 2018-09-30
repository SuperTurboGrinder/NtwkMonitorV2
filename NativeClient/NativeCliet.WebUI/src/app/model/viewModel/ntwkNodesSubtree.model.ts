import { NtwkNodeDataContainer } from "./ntwkNodeDataContainer.model";

export class NtwkNodesSubtree {
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