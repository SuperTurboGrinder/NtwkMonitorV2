import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";
import { SettingsProfilesService } from "../../services/settingsProfiles.service";
import { SettingsProfile } from "../../model/httpModel/settingsProfile.model";
import { ActivatedRoute } from "@angular/router";
import { MessagingService } from "src/app/services/messaging.service";
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";
import { BaseCrudSelectorComponent } from "../helpers/baseCrudSelectorComponent.helper";

@Component({
    selector: 'settingsProfileSelection',
    templateUrl: './settingsProfileSelection.component.html'
})
export class SettingsProfileSelectionComponent
    extends BaseCrudSelectorComponent<SettingsProfile, SettingsProfilesService> {
    public readonly isEditorView: boolean;

    constructor(
        messager: MessagingService,
        private settingsService: SettingsProfilesService,
        route: ActivatedRoute
    ) {
        super(messager, settingsService);
        let pageName = route.snapshot.url[0].path;
        this.isEditorView = pageName !== "profilesSelect";
    }

    public setAndContinue(profileID: number) {
        if(!this.isCurrentProfile(profileID)) {
            this.settingsService.setCurrentProfile(profileID);
            console.log(`Set ${profileID} as current profile.`);
        }
    }

    public isCurrentProfile(id: number): boolean {
        return this.settingsService.isCurrentProfileID(id);
    }

    protected updateDataList() {
        this.dataService.getProfiles().subscribe(
            tagsListResult => this.setNewData(tagsListResult)
        );
    }

    protected deleteObjectPermanently(
        objID: number,
        callback: (success: boolean) => void
    ) {
        this.dataService.deleteProfile(objID, callback);
    }
}