import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { NodeTag } from 'src/app/model/httpModel/nodeTag.model';
import { TagsService } from '../../services/tags.service';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';

@Component({
    selector: 'app-tags-binding',
    templateUrl: './tagsBinding.component.html'
})
export class TagsBindingComponent {
    private tags: NodeTag[] = null;
    availableTags: NodeTag[] = [];
    setTags: NodeTag[] = [];
    private _originalSetTags: number[] = null;

    @Output() private applyEvent = new EventEmitter<number[]>();

    constructor(
        private messager: MessagingService,
        private tagsService: TagsService
    ) {
        this.updateTagsList();
    }

    @Input() set originalSetTags(tagsIDs: number[]) {
        if (tagsIDs !== null && this._originalSetTags !== tagsIDs) {
            this._originalSetTags = tagsIDs.sort((a, b) => a - b);
            if (this.tags !== null) {
                this.updateSetTags();
            }
        }
    }

    public get isNoSelection(): boolean {
        return this._originalSetTags === null;
    }

    public selectTag(availableTagsIndex: number) {
        this.swapTags(availableTagsIndex, this.availableTags, this.setTags);
    }

    public deselectTag(setTagsIndex: number) {
        this.swapTags(setTagsIndex, this.setTags, this.availableTags);
    }

    private swapTags(
        fromIndex: number,
        fromArray: NodeTag[],
        toArray: NodeTag[]
    ) {
        const tagID = fromArray[fromIndex];
        fromArray.splice(fromIndex, 1);
        toArray.push(tagID);
        toArray.sort((a, b) => a.id - b.id);
    }

    updateSetTags() {
        if (this._originalSetTags !== null) {
            this.setTags = this.tags
                .filter(t => this._originalSetTags.includes(t.id))
                .sort((a, b) => a.id - b.id);
            this.availableTags = this.tags
                .filter(t => this._originalSetTags.indexOf(t.id) === -1)
                .sort((a, b) => a.id - b.id);
        }
    }

    private updateTagsList() {
        this.tagsService.getTagsList().subscribe(
            (tagsResult: HTTPResult<NodeTag[]>) => {
                if (tagsResult.success === true) {
                    this.tags = tagsResult.data;
                    this.updateSetTags();
                }
            }
        );
    }

    public applyChanges() {
        if (this.isArrayChanged()) {
            this.applyEvent.emit(this.setTags.map(t => t.id));
        } else {
            this.messager.showMessage(MessagesEnum.NoChangesDetected);
        }
    }

    private isArrayChanged() {
        if (this._originalSetTags.length !== this.setTags.length) {
            return true;
        } else {
            for (let i = 0; i < this.setTags.length; i++) {
                if (this._originalSetTags[i] !== this.setTags[i].id) {
                    return true;
                }
            }
        }
        return false;
    }
}
