import { NtwkNodeDataContainer } from "./ntwkNodeDataContainer.model";
import { AllNodesData } from "../httpModel/allNodesData.model";
import { NodeData } from "../httpModel/nodeData.model";
import { CWSData } from "../httpModel/cwsData.model";

export class NtwkNodesTree {
    public webServicesNames: CWSData[] = [];
    public treeLayers: NtwkNodeDataContainer[][] = [];
    public allNodes: NtwkNodeDataContainer[] = [];

    public constructor(allNodesData: AllNodesData) {
        this.webServicesNames = allNodesData.webServicesData;
        let index: number = 0;
        this.treeLayers = allNodesData.nodesData.map((layer, layer_index) =>
            layer.map(nodeData =>
                new NtwkNodeDataContainer(index++, layer_index, nodeData)
            )
        );
        this.allNodes = this.treeLayers.reduce(
            (acc, layer) => acc = acc.concat(layer),
            []
        );
        this.buildTreeBranches();
    }

    private buildTreeBranches() {
        if(this.treeLayers.length > 1) {
            for(var lIndex = 1; lIndex < this.treeLayers.length; lIndex++) {
                var prevLayer = this.treeLayers[lIndex-1];
                var currentLayer = this.treeLayers[lIndex];
                currentLayer.forEach(nodeData => {
                    var parentID = nodeData.nodeData.node.parentID;
                    var parent = prevLayer.find(
                        parentNodeData =>
                            parentNodeData.nodeData.node.id == parentID
                    );
                    parent.children.push(nodeData);
                    nodeData.parent = parent;
                })
            }
        }
    }
}