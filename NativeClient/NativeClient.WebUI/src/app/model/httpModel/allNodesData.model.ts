import { NodeData } from './nodeData.model';
import { CWSData } from './cwsData.model';

export class AllNodesData {
    constructor(
        public webServicesData: CWSData[],
        public nodesData: NodeData[][]
    ) {}
}
