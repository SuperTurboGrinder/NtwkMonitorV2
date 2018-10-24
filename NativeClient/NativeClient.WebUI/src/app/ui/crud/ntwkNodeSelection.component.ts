import { Component } from '@angular/core';
import { DisplayTreeHelper } from '../helpers/displayTreeHelper.helper';
import { NodesService } from '../../services/nodes.service';
import { NtwkNode } from '../../model/httpModel/ntwkNode.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudSelectorComponent } from '../helpers/baseCrudSelectorComponent.helper';
import { HTTPResult } from 'src/app/model/servicesModel/httpResult.model';
import { NodeLineData } from 'src/app/model/viewModel/nodeLineData.model';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';

@Component({
    selector: 'app-node-selection',
    templateUrl: './ntwkNodeSelection.component.html'
})
export class NtwkNodeSelectionComponent
    extends BaseCrudSelectorComponent<NodeLineData, NodesService> {
    public displayLocalDeleteMessage = false;
    public displayLocalMoveMessage = false;
    private nodeToRemove: NodeLineData = null;
    private nodeToMove: NodeLineData = null;
    private nodeToMoveNewParent: NodeLineData = null;

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

    public get isInMoveMode() {
        return this.nodeToMove !== null;
    }

    public isNodeToMove(nodeID: number) {
        return this.nodeToMove.id === nodeID;
    }

    public isNodeToMoveOnRoot() {
        return this.nodeToMove.depth === 0;
    }

    public setMoveModeFor(nodeToMove: NodeLineData) {
        this.nodeToMove = nodeToMove;
    }

    public tryToMove(newParent: NodeLineData) {
        this.nodeToMoveNewParent = newParent;
        this.displayLocalMoveMessage = true;
    }

    public moveHandler(shouldMove: boolean) {
        this.displayLocalMoveMessage = false;
        this.moveNodeToNewParent(shouldMove);
    }

    private moveNodeToNewParent(shouldMove: boolean) {
        if (shouldMove) {
            this.displayOperationInProgress = true;
            this.dataService.moveNodeToNewParent(
                this.nodeToMove.id,
                this.nodeToMoveNewParent === null
                    ? 0
                    : this.nodeToMoveNewParent.id,
                success => {
                    this.confirmOperationSuccess(
                        success,
                        MessagesEnum.NodeMovedSuccessfully
                    );
                    if (success) {
                        this.nodeToMove = null;
                    }
                }
            );
        }
    }

    public get nodeToMoveName() {
        return this.nodeToMove === null
            ? '' : this.nodeToMove.name;
    }

    public get nodeToMoveNewParentName() {
        return this.nodeToMoveNewParent === null
            ? 'root' : this.nodeToMoveNewParent.name;
    }
}
