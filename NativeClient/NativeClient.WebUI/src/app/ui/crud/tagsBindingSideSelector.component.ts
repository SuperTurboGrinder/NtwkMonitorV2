import { Component, OnDestroy } from '@angular/core';
import { NodesService } from '../../services/nodes.service';
import { MessagingService } from 'src/app/services/messaging.service';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';
import { BaseCrudNodeSideSelectorComponent } from '../helpers/baseCrudNodeSideSelectorComponent.helper';
import { NodeInfoPopupDataService } from 'src/app/services/nodeInfoPopupData.service';
import { TagsService } from 'src/app/services/tags.service';
import { SettingsProfilesService } from 'src/app/services/settingsProfiles.service';

@Component({
    selector: 'app-tags-binding-side-selector',
    templateUrl: './tagsBindingSideSelector.component.html'
})
export class TagsBindingSideSelectorComponent
    extends BaseCrudNodeSideSelectorComponent implements OnDestroy {

    constructor(
        nodeInfoPopupService: NodeInfoPopupDataService,
        tagsService: TagsService,
        messager: MessagingService,
        dataService: NodesService,
        private settingsService: SettingsProfilesService
    ) {
        super(nodeInfoPopupService, tagsService, messager, dataService);
    }

    public ngOnDestroy() {
        this.settingsService.refreshCurrentProfile();
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
