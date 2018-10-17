import { Component } from "@angular/core";
import { TagsService } from "../../services/tags.service";
import { NodeTag } from "../../model/httpModel/nodeTag.model";
import { MessagingService } from "src/app/services/messaging.service";
import { BaseCrudSelectorComponent } from "../helpers/baseCrudSelectorComponent.helper";

@Component({
    selector: 'tagSelection',
    templateUrl: './tagSelection.component.html'
})
export class TagSelectionComponent
    extends BaseCrudSelectorComponent<NodeTag, TagsService> {
    constructor(
        messager: MessagingService,
        dataService: TagsService
    ) {
        super(messager, dataService);
    }

    protected updateDataList() {
        this.dataService.getTagsList().subscribe(
            tagsListResult => this.setNewData(tagsListResult)
        );
    }

    protected deleteObjectPermanently(
        objID: number,
        callback: (success: boolean) => void
    ) {
        this.dataService.deleteTag(objID, callback);
    }
}