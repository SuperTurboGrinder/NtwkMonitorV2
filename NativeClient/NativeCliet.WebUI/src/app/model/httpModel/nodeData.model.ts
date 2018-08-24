import { NtwkNode } from "./ntwkNode.model";

export class NodeData {
    constructor(
        public node: NtwkNode,
        public tagsIDs: number[],
        public boundWebServicesIDs: number[]
    ) {}
}