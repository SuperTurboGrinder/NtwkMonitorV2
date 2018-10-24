import { Component, Input } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { NodeTag } from 'src/app/model/httpModel/nodeTag.model';
import { TagsService } from '../../services/tags.service';
import { BaseCrudFormComponent } from '../helpers/baseCrudFormComponent.helper';

@Component({
    selector: 'app-tags-binding',
    templateUrl: './tagsBinding.component.html'
})
export class TagsBindingComponent {
    tags: NodeTag[] = null;
    availableTags: NodeTag[] = [];
    setTags: NodeTag[] = [];
    private _originalSetTags: number[] = [];

    constructor(
        private messager: MessagingService,
        private tagsService: TagsService
    ) {
        this.updateTagsList();
    }

    @Input() set originalSetTags(tagsIDs: number[]) {
        if (this._originalSetTags !== tagsIDs) {
            this._originalSetTags = tagsIDs;
            if (this.tags !== null) {
                this.updateSetTags();
            }
        }
    }

    updateSetTags() {
        this.setTags = this.tags.filter(t => this._originalSetTags.includes(t.id));
        this.availableTags = this.tags.filter(t => this._originalSetTags.indexOf(t.id) === -1);
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
}
