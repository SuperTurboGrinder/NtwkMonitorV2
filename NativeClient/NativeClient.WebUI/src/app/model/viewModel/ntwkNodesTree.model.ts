import { NtwkNodeDataContainer } from './ntwkNodeDataContainer.model';
import { AllNodesData } from '../httpModel/allNodesData.model';
import { CWSData } from '../httpModel/cwsData.model';
import * as _ from 'lodash';

export class NtwkNodesTree {
    public webServicesNames: CWSData[] = [];
    public treeLayers: NtwkNodeDataContainer[][] = [];
    public allNodes: NtwkNodeDataContainer[] = [];

    public constructor(allNodesData: AllNodesData) {
        this.webServicesNames = allNodesData.webServicesData;
        let index = 0;
        this.treeLayers = allNodesData.nodesData.map(
            (layer, layer_index) => layer.map(
                nodeData => new NtwkNodeDataContainer(index++, layer_index, nodeData)
            )
        );
        this.allNodes = _.flatten(this.treeLayers);
        this.buildTreeBranches();
    }

    private buildTreeBranches() {
        if (this.treeLayers.length > 1) {
            for (let lIndex = 1; lIndex < this.treeLayers.length; lIndex++) {
                const prevLayer = this.treeLayers[lIndex - 1];
                const currentLayer = this.treeLayers[lIndex];
                const groupedByParent = _.groupBy(
                    currentLayer,
                    n => n.nodeData.node.parentId
                );
                for (const parent of prevLayer) {
                    const children = groupedByParent[parent.nodeData.node.id];
                    if (children !== undefined) {
                        parent.children = children;
                        for (const child of children) {
                            child.parent = parent;
                        }
                    }
                }
            }
        }
    }
}
