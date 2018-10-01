import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";
import { SettingsProfilesService } from "../services/settingsProfiles.service";
import { SettingsProfile } from "../model/httpModel/settingsProfile.model";

@Component({
    selector: 'settingsProfileSelection',
    templateUrl: './settingsProfileSelection.component.html'
})
export class SettingsProfileSelectionComponent {
    private profiles: SettingsProfile[] = null;
    private loadingError: boolean = false;

    constructor(
        private settingsService: SettingsProfilesService
    ) {
        this.updateProfilesList();
    }

    get profilesList() : SettingsProfile[] {
        return this.profiles;
    }

    get isLoadingError() : boolean {
        return this.loadingError;
    }

    public setAndContinue(profileID: number) {
        this.settingsService.setCurrentProfile(profileID);
        console.log(`Set ${profileID} as current profile.`);
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