import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { TagsService } from "../../services/tags.service";
import { NodeTag } from "../../model/httpModel/nodeTag.model";

@Component({
    selector: 'tagSelection',
    templateUrl: './tagSelection.component.html'
})
export class TagSelectionComponent {
    private tags: NodeTag[] = null;
    private loadingError = false;

    constructor(
        private tagsService: TagsService
    ) {
        this.updateTagsList();
    }

    private updateTagsList() {
        this.tagsService.getTagsList().subscribe(tagsListResult => {
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
}