import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { TagsService } from "../../services/tags.service";
import { NodeTag } from "../../model/httpModel/nodeTag.model";
import { MessagingService } from "src/app/services/messaging.service";
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";

@Component({
    selector: 'tagSelection',
    templateUrl: './tagSelection.component.html'
})
export class TagSelectionComponent {
    private tags: NodeTag[] = null;
    private loadingError = false;
    private tagToRemove: NodeTag = null;
    private aboutToBeRemovedTagName = "";
    public displayDeleteMessage = false;
    public displayOperationInProgress = false;

    constructor(
        private messager: MessagingService,
        private tagsService: TagsService
    ) {
        this.updateTagsList();
    }

    private updateTagsList() {
        this.tagsService.getTagsList().subscribe(tagsListResult => {
            console.log(tagsListResult)
            if(tagsListResult.success === true) {
                this.tags = tagsListResult.data;
            } else {
                this.loadingError = true;
            }
        });
    }

    public get tagsList() {
        return this.tags;
    }

    public tagsTrackBy(index: number, tag: NodeTag) {
        return tag.id;
    }

    get isLoadingError() : boolean {
        return this.loadingError;
    }

    public refresh(_: boolean) {
        this.updateTagsList();
    }

    public tryRemove(tagToRemove: NodeTag) {
        this.tagToRemove = tagToRemove;
        this.aboutToBeRemovedTagName = tagToRemove.name;
        this.displayDeleteMessage = true;
    }

    public deleteTag(shouldDelete: boolean) {
        this.displayDeleteMessage = false;
        if(shouldDelete && this.tagToRemove !== null) {
            this.displayOperationInProgress = true;
            this.tagsService.deleteTag(
                this.tagToRemove.id,
                (success: boolean) => {
                    let removed = this.tagToRemove;
                    if(success === true) {
                        this.tags = this.tags.filter(t => t.id != removed.id);
                        this.messager.showMessage(MessagesEnum.DeletedSuccessfully);
                    }
                    this.displayOperationInProgress = false;
                }
            );
        }
    }
}