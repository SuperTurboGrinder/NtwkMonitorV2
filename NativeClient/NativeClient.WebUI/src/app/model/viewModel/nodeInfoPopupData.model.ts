import { NtwkNode } from '../httpModel/ntwkNode.model';

export class NodeInfoPopupData {
    node: NtwkNode;
    webServicesNames: string[];
    tagsNames: string[];
    screenPos: {x: number, y: number};
}
