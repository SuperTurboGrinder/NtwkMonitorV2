import { RawNodeData } from "./rawNodeData.model";
import { CWSData } from "./cwsData.model";

export class AllRawNodesData {
    constructor(
        public webServicesData: CWSData[],
        public nodesData: RawNodeData[]
    ) {}
}