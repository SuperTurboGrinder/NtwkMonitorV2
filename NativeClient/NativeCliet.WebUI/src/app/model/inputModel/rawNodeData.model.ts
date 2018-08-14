import { NtwkNode } from "./ntwkNode.model";

export class RawNodeData {
    constructor(
        public node: NtwkNode,
        public tagsIDs: number[],
        public boundWebServicesIDs: number[]
    ) {}
}