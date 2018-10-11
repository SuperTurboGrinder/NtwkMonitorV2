import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";
import { SettingsProfilesService } from "../../services/settingsProfiles.service";
import { SettingsProfile } from "../../model/httpModel/settingsProfile.model";
import { ActivatedRoute } from "@angular/router";
import { MessagingService } from "src/app/services/messaging.service";
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";

@Component({
    selector: 'settingsProfileSelection',
    templateUrl: './settingsProfileSelection.component.html'
})
export class SettingsProfileSelectionComponent {
    private profiles: SettingsProfile[] = null;
    private loadingError = false;
    public readonly isEditorView: boolean;
    public displayOperationInProgress = false;

    private profileToRemove: SettingsProfile = null;
    public aboutToBeRemovedProfileName = "";
    public displayDeleteMessage = false;

    constructor(
        private messager: MessagingService,
        private settingsService: SettingsProfilesService,
        route: ActivatedRoute
    ) {
        let pageName = route.snapshot.url[0].path;
        this.isEditorView = pageName !== "profilesSelect";
        this.updateProfilesList();
    }

    public profilesTrackBy(index: number, profile: SettingsProfile) {
        return profile.id;
    }

    get profilesList() : SettingsProfile[] {
        return this.profiles;
    }

    get isLoadingError() : boolean {
        return this.loadingError;
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

    private updateProfilesList() {
        this.settingsService.getProfiles().subscribe(
            profilesResult => {
                if(profilesResult.success === false) {
                    this.loadingError = true;
                    return;
                }
                this.profiles = profilesResult.data;
            }
        )
    }

    public refresh(_: boolean) {
        this.updateProfilesList();
    }

    public tryRemove(profileToRemove: SettingsProfile) {
        this.profileToRemove = profileToRemove;
        this.aboutToBeRemovedProfileName = profileToRemove.name;
        this.displayDeleteMessage = true;
    }

    public deleteProfile(shouldDelete: boolean) {
        this.displayDeleteMessage = false;
        if(shouldDelete && this.profileToRemove !== null) {
            this.displayOperationInProgress = true;
            this.settingsService.deleteProfile(
                this.profileToRemove.id,
                (success: boolean) => {
                    let removed = this.profileToRemove;
                    if(success === true) {
                        this.profiles = this.profiles.filter(p => p.id != removed.id);
                        this.messager.showMessage(MessagesEnum.DeletedSuccessfully);
                    }
                    this.displayOperationInProgress = false;
                }
            );
        }
    }
}