import { Component } from "@angular/core";
import { Location } from "@angular/common";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { HTTPResult } from "../../model/servicesModel/httpResult.model";
import { Range } from "../misc/numRangeSelector.component"
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";
import { MessagingService } from "src/app/services/messaging.service";
import { NodeTag } from "src/app/model/httpModel/nodeTag.model";
import { TagsService } from "../../services/tags.service";

@Component({
    selector: 'tagForm',
    templateUrl: './tagForm.component.html'
})
export class TagFormComponent {
    private _isEditMode: boolean = false;
    public original: NodeTag = null;
    public tag: NodeTag = null;

    public displayOperationInProgress = false;
    public displayDiscardMessage = false;
    public displayCreateConfirmationMessage = false;
    public displaySaveChangesMessage = false;

    public get isEditMode() {
        return this._isEditMode;
    }

    constructor(
        private messager: MessagingService,
        private location: Location,
        route: ActivatedRoute,
        private tagsService: TagsService
    ) {
        console.log("TEST")
        this.tag = {
            id: 0,
            name: ""
        }
        let routeSnapshot: ActivatedRouteSnapshot = route.snapshot;
        this._isEditMode = routeSnapshot.url[1].path === "edit";
        if(this._isEditMode) {
            let selectedID = parseInt(routeSnapshot.params.id);
            tagsService.getTagsList().subscribe(
                (tagsResult: HTTPResult<NodeTag[]>) => {
                    if(tagsResult.success === true) {
                        this.original = tagsResult.data.find(
                            tag => tag.id === selectedID
                        );
                        this.tag = {
                            id: this.original.id,
                            name: this.original.name
                        }
                    }
                }
            );
        } else {
            this.original = {
                id: this.tag.id,
                name: this.tag.name
            }
        }
    }

    private isOkToDiscard(): boolean {
        return this.original.name === this.tag.name
    }

    public tryDiscard() {
        if(this.isOkToDiscard())
            this.discardAndReturn(true);
        else
            this.displayDiscardMessage = true;
    }

    public discardAndReturn(shouldDiscard: boolean) {
        this.displayDiscardMessage = false;
        if(shouldDiscard) {
            this.location.back();
        }
    }

    private confirmOperationSuccess(
        success: boolean,
        successMessage: MessagesEnum
    ) {
        if(success) {
            this.messager.showMessage(successMessage);
            this.location.back();
        } else {
            this.displayOperationInProgress = false;
        }
    }

    public tryCreateNew() {
        this.displayCreateConfirmationMessage = true;
    }

    public createNewTagAndReturn(shouldCreate: boolean) {
        this.displayCreateConfirmationMessage = false;
        if(!shouldCreate) return;
        this.displayOperationInProgress = true;
        this.tagsService.createNewTag(
            this.tag,
            (success: boolean) => this.confirmOperationSuccess(
                success,
                MessagesEnum.CreatedSuccessfully
            )
        );
    }

    public trySaveChanges() {
        if(this.isOkToDiscard())
            this.messager.showMessage(MessagesEnum.NoChangesDetected);
        else
            this.displaySaveChangesMessage = true;
    }

    public saveChangesToTagAndReturn(shouldSave: boolean) {
        this.displaySaveChangesMessage = false;
        if(!shouldSave) return;
        this.displayOperationInProgress = true;
        this.tagsService.updateTag(
            this.tag,
            (success: boolean) => this.confirmOperationSuccess(
                success,
                MessagesEnum.UpdatedSuccessfully
            )
        );
    }
}