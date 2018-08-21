import { NtwkNode } from "../httpModel/ntwkNode.model";

export class NtwkNodeDataContainer {
    constructor(
        public node: NtwkNode,
        public parent: NtwkNodeDataContainer,
        public children: NtwkNodeDataContainer,
        public tagsIDs: number[],
        public webServicesIDs: number[]
    ) {}
}