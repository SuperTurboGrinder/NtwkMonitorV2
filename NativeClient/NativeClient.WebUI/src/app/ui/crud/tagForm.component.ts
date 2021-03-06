import { Component, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { NodeTag } from 'src/app/model/httpModel/nodeTag.model';
import { TagsService } from '../../services/tags.service';
import { BaseCrudFormComponent } from '../helpers/baseCrudFormComponent.helper';
import { SettingsProfilesService } from 'src/app/services/settingsProfiles.service';

@Component({
    selector: 'app-tag-form',
    templateUrl: './tagForm.component.html'
})
export class TagFormComponent
    extends BaseCrudFormComponent<NodeTag, TagsService> implements OnDestroy {

    constructor(
        messager: MessagingService,
        location: Location,
        route: ActivatedRoute,
        tagsService: TagsService,
        private settingsService: SettingsProfilesService
    ) {
        super(messager, location, route, tagsService);
    }

    public ngOnDestroy() {
        this.settingsService.refreshCurrentProfile();
    }

    protected getOriginalData(
        id: number,
        callback: (success: boolean, orig: NodeTag) => void
    ) {
        this.dataService.getTagsList().subscribe(
            (tagsResult: HTTPResult<NodeTag[]>) => {
                const tag = tagsResult.success === true
                    ? tagsResult.data.find(t => t.id === id)
                    : null;
                callback(
                    tagsResult.success,
                    tag
                );
            }
        );
    }

    protected newEmptyData(): NodeTag {
        return { id: 0, name: '' };
    }

    protected currentIdenticalTo(obj: NodeTag): boolean {
        return obj.name === this.data.name;
    }

    protected makeCopy(orig: NodeTag): NodeTag {
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
