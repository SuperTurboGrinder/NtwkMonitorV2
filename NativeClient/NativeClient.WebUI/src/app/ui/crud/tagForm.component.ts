import { Component } from "@angular/core";
import { Location } from "@angular/common";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { HTTPResult } from "../../model/servicesModel/httpResult.model";
import { Range } from "../misc/numRangeSelector.component"
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";
import { MessagingService } from "src/app/services/messaging.service";
import { NodeTag } from "src/app/model/httpModel/nodeTag.model";
import { TagsService } from "../../services/tags.service";
import { BaseCrudFormComponent } from "../helpers/baseCrudFormComponent.helper";

@Component({
    selector: 'tagForm',
    templateUrl: './tagForm.component.html'
})
export class TagFormComponent
    extends BaseCrudFormComponent<NodeTag, TagsService> {
    
    constructor(
        messager: MessagingService,
        location: Location,
        route: ActivatedRoute,
        tagsService: TagsService
    ) {
        super(messager, location, route, tagsService);
    }

    protected getOriginalData(
        id: number,
        callback: (success: boolean, orig: NodeTag) => void
    ) {
        this.dataService.getTagsList().subscribe(
            (tagsResult: HTTPResult<NodeTag[]>) => {
                let tag = tagsResult.success === true
                    ? tagsResult.data.find(tag => tag.id === id)
                    : null;
                callback(
                    tagsResult.success,
                    tag
                );
            }
        )
    }

    protected newEmptyData(): NodeTag {
        return { id: 0, name: "" };
    }

    protected currentIdenticalTo(obj: NodeTag): boolean {
        return obj.name === this.data.name;
    }

    protected makeCopy(orig: NodeTag) : NodeTag {
        return {
            id: orig.id,
            name: orig.name
        };
    }

    protected saveAsNewObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.createNewTag(
            this.data,
            callback
        );
    }

    protected saveChangesToObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.updateTag(
            this.data,
            callback
        );
    }
}