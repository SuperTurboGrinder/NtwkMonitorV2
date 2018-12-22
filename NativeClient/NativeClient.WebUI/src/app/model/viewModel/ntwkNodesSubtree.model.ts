import { NtwkNodeDataContainer } from './ntwkNodeDataContainer.model';
import { BaseObjectTree } from './baseObjectTree';

export class NtwkNodesSubtree implements BaseObjectTree {
    constructor(
        public container: NtwkNodeDataContainer,
        public isBranchPingable: boolean,
        public children: NtwkNodesSubtree[]
    ) {}

    public get isPingable(): boolean {
        return this.container.nodeData.node.isOpenPing;
    }
}
