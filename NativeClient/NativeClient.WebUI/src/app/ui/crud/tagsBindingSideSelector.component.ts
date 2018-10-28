import { Component } from '@angular/core';
import { NodesService } from '../../services/nodes.service';
import { MessagingService } from 'src/app/services/messaging.service';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';
import { BaseCrudNodeSideSelectorComponent } from '../helpers/baseCrudNodeSideSelectorComponent.helper';

@Component({
    selector: 'app-tags-binding-side-selector',
    templateUrl: './tagsBindingSideSelector.component.html'
})
export class TagsBindingSideSelectorComponent
    extends BaseCrudNodeSideSelectorComponent {

    constructor(
        messager: MessagingService,
        dataService: NodesService
    ) {
        super(messager, dataService);
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
}
