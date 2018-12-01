import { NtwkNode } from '../httpModel/ntwkNode.model';

export class NodeInfoPopupData {
    node: NtwkNode;
    showExecServices: boolean;
    showWebServices: boolean;
    showTags: boolean;
    webServicesNames: string[];
    tagsNames: string[];
    screenPos: {x: number, y: number};
}
