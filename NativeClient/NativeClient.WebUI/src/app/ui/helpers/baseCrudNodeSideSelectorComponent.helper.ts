import { Input } from '@angular/core';
import { DisplayTreeHelper } from '../helpers/displayTreeHelper.helper';
import { NodesService } from '../../services/nodes.service';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudSelectorComponent } from '../helpers/baseCrudSelectorComponent.helper';
import { HTTPResult } from 'src/app/model/servicesModel/httpResult.model';
import { NodeLineData } from 'src/app/model/viewModel/nodeLineData.model';
import { NodeData } from 'src/app/model/httpModel/nodeData.model';

export class BaseCrudNodeSideSelectorComponent
    extends BaseCrudSelectorComponent<NodeLineData, NodesService> {

    private allNodesData: NodeData[] = null;
    protected selectedNodeData: NodeData = null;

    @Input() isOperationInProgress = true;

    constructor(
        messager: MessagingService,
        dataService: NodesService
    ) {
        super(messager, dataService);
    }

    public selectNode(nodeID: number) {
        const nodeData = this.allNodesData.find(nd => nd.node.id === nodeID);
        this.selectedNodeData = nodeData;
    }

    protected updateDataList() {
        this.dataService.getNodesTree().subscribe(treeResult => {
            if (treeResult.success === false) {
                this.setNewData(HTTPResult.Failed());
            } else if (treeResult.data.nodesTree.allNodes.length === 0) {
                this.setNewData(HTTPResult.Successful([]));
            } else {
                const treeLayersCount = treeResult.data.nodesTree.treeLayers.length;
                const displayTreeHelper = new DisplayTreeHelper(
                    null,
                    treeResult.data.nodesTree.allNodes,
                    treeResult.data.nodesTree.treeLayers,
                    0,
                    treeLayersCount
                );
                this.allNodesData = treeResult.data.nodesTree.allNodes
                    .map(c => c.nodeData);
                this.setNewData(HTTPResult.Successful(
                    displayTreeHelper.displayedNodesIndexes.map(
                        (dni, index) => {
                            const nodeContainer = treeResult.data.nodesTree.allNodes[dni];
                            const node = nodeContainer.nodeData.node;
                            const prefix = displayTreeHelper.prefix(index);
                            const depth = nodeContainer.depth;
                            return new NodeLineData(
                                node.id,
                                node.name,
                                prefix,
                                depth,
                                nodeContainer.children.length > 0
                            );
                        }
                    )
                ));
                if (this.selectedNodeData !== null) {
                    this.selectNode(this.selectedNodeData.node.id);
                }
            }
        });
    }

    protected deleteObjectPermanently(
        nodeID: number,
        callback: (success: boolean) => void
    ) { } // not used
}
