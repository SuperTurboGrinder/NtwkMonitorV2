import { SmartNodeContainer } from "./nodesPacking";

export interface Implementation {
    readonly nodesNames: string[];
    useNodes(nodeContainers: SmartNodeContainer[]): void;
}