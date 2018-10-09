import { Component } from "@angular/core";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { NodeTag } from "../../model/httpModel/nodeTag.model";
import { TagsService } from "../../services/tags.service";

@Component({
    selector: 'tagForm',
    templateUrl: './tagForm.component.html'
})
export class TagFormComponent {
    public tag: NodeTag;

    constructor(
        tagsService: TagsService
    ) {}
}