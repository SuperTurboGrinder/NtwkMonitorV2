import { Component, Input } from "@angular/core";

import { NodesRepository } from "../services/nodes.repository";
import { NodeData } from "../model/httpModel/nodeData.model";

@Component({
    selector: 'taggedNodeList',
    templateUrl: './taggedNodeList.component.html'
})
export class TaggedNodeListComponent {
    private filteredNodesList: NodeData[] = null;
    public sortedIndexes: number[] = null;

    getNodeData(i:number) : NodeData {
        return this.filteredNodesList[i];
    }

    constructor(nodesRepo: NodesRepository) {
        nodesRepo.getNodesTree().subscribe(tree => {
            console.log("TEST")
            let allNodes = tree.allNodes;
            this.filteredNodesList = allNodes.map(nData =>
                nData.nodeData
            );
            this.sortedIndexes = Array.from(
                {length: allNodes.length},
                (_,i) => i
            );
        });
    }
}