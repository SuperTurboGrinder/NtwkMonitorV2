import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";
import { SettingsProfilesService } from "../../services/settingsProfiles.service";
import { SettingsProfile } from "../../model/httpModel/settingsProfile.model";
import { ActivatedRoute } from "@angular/router";

@Component({
    selector: 'settingsProfileSelection',
    templateUrl: './settingsProfileSelection.component.html'
})
export class SettingsProfileSelectionComponent {
    private profiles: SettingsProfile[] = null;
    private loadingError = false;
    public readonly isEditorView: boolean;

    constructor(
        private settingsService: SettingsProfilesService,
        route: ActivatedRoute
    ) {
        let pageName = route.snapshot.url[0].path;
        this.isEditorView = pageName !== "profilesSelect";
        this.updateProfilesList();
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
}