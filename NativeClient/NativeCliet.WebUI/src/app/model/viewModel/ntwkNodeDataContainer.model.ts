import { NtwkNode } from "../httpModel/ntwkNode.model";
import { NodeData } from "../httpModel/nodeData.model";
import { AllNodesData } from "../httpModel/allNodesData.model";

export class NtwkNodeDataContainer {
    constructor(
        public index: number, //index from all nodes list
        public depth: number, //tree layer from 0
        public nodeData: NodeData,
        public parent: NtwkNodeDataContainer = null,
        public children: NtwkNodeDataContainer[] = []
    ) {}
}