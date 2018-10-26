import { Component } from '@angular/core';
import { DisplayTreeHelper } from '../helpers/displayTreeHelper.helper';
import { NodesService } from '../../services/nodes.service';
import { NtwkNode } from '../../model/httpModel/ntwkNode.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudSelectorComponent } from '../helpers/baseCrudSelectorComponent.helper';
import { HTTPResult } from 'src/app/model/servicesModel/httpResult.model';
import { NodeLineData } from 'src/app/model/viewModel/nodeLineData.model';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';
import { NodeData } from 'src/app/model/httpModel/nodeData.model';

@Component({
    selector: 'app-tags-binding-form-selection',
    templateUrl: './tagsBindingForm.component.html'
})
export class TagsBindingFormComponent
    extends BaseCrudSelectorComponent<NodeLineData, NodesService> {

    private allNodesData: NodeData[] = null;
    private selectedNodeData: NodeData = null;
    public currentTagsSet: number[] = [];

    constructor(
        messager: MessagingService,
        dataService: NodesService
    ) {
        super(messager, dataService);
    }

    public selectNode(node: NodeLineData) {
        const nodeData = this.allNodesData.find(nd => nd.node.id === node.id);
        this.selectedNodeData = nodeData;
    }

    public isSelectedNode(nodeID: number) {
        return this.selectedNodeData !== null
            ? this.selectedNodeData.node.id === nodeID
            : false;
    }

    public get selectedNodeTagsSet(): number[] {
        return this.selectedNodeData !== null
            ? this.selectedNodeData.tagsIDs
            : null;
    }

    public setSelectedNodeTags(tagsIDs: number[]) {
        if (this.selectedNodeData !== null) {
            this.displayOperationInProgress = true;
            this.dataService.setNodeTags(
                this.selectedNodeData.node.id,
                tagsIDs,
                success => {
                    if (success === true) {
                        this.selectedNodeData.tagsIDs = tagsIDs;
                    }
                    this.confirmOperationSuccess(
                        success,
                        MessagesEnum.SetTagsSuccessfully
                    );
                }
            );
        }
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
                            return new NodeLineData(node.id, node.name, prefix, depth);
                        }
                    )
                ));
            }
        });
    }

    protected deleteObjectPermanently(
        nodeID: number,
        callback: (success: boolean) => void
    ) { } // not used
}
