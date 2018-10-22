import { Component } from '@angular/core';
import { DisplayTreeHelper } from '../helpers/displayTreeHelper.helper';
import { NodesService } from '../../services/nodes.service';
import { NtwkNode } from '../../model/httpModel/ntwkNode.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudSelectorComponent } from '../helpers/baseCrudSelectorComponent.helper';
import { HTTPResult } from 'src/app/model/servicesModel/httpResult.model';

class NodeLineData {
    constructor(
        public id: number,
        public name: string,
        public prefix: string
    ) { }
}

@Component({
    selector: 'app-node-selection',
    templateUrl: './ntwkNodeSelection.component.html'
})
export class NtwkNodeSelectionComponent
    extends BaseCrudSelectorComponent<NodeLineData, NodesService> {
    public displayLocalDeleteMessage = false;
    private nodeToRemove: NodeLineData = null;

    constructor(
        messager: MessagingService,
        dataService: NodesService
    ) {
        super(messager, dataService);
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
                this.setNewData(HTTPResult.Successful(
                    displayTreeHelper.displayedNodesIndexes.map(
                        (dni, index) => {
                            const node = displayTreeHelper.node(dni);
                            const prefix = displayTreeHelper.prefix(index);
                            return new NodeLineData(node.id, node.name, prefix);
                        }
                    )
                ));
            }
        });
    }

    protected deleteObjectPermanently(
        nodeID: number,
        callback: (success: boolean) => void
    ) {
        this.dataService.deleteNode(nodeID, callback);
    }

    public tryRemove(objectToRemove: NodeLineData) {
        this.nodeToRemove = objectToRemove;
        this.displayLocalDeleteMessage = true;
    }

    public deleteHandler(shouldDelete: boolean) {
        this.displayLocalDeleteMessage = false;
        this.deleteObject({
            shouldDelete: shouldDelete,
            id: this.nodeToRemove.id
        });
    }

    public get nodeToRemoveName() {
        return this.nodeToRemove === null
            ? '' : this.nodeToRemove.name;
    }
}
