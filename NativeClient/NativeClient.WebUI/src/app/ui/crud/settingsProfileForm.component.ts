import { Component } from "@angular/core";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { SettingsProfile } from "../../model/httpModel/settingsProfile.model";
import { SettingsProfilesService } from "../../services/settingsProfiles.service";
import { HTTPResult } from "../../model/servicesModel/httpResult.model";

@Component({
    selector: 'settingsProfileForm',
    templateUrl: './settingsProfileForm.component.html'
})
export class SettingsProfileFormComponent {
    private _isEditMode: boolean = false;
    public profile: SettingsProfile = null;

    public get isEditMode() {
        return this._isEditMode;
    }

    constructor(
        route: ActivatedRoute,
        settingsService: SettingsProfilesService
    ) {
        this.profile = new SettingsProfile(
            0, "", false, null, 0, 23, true, true, 1
        );
        let routeSnapshot: ActivatedRouteSnapshot = route.snapshot;
        this._isEditMode = routeSnapshot.url[1].path === "edit";
        if(this._isEditMode) {
            let selectedID = parseInt(routeSnapshot.params.id);
            console.log(selectedID);
            settingsService.getProfiles().subscribe(
                (profilesResult: HTTPResult<SettingsProfile[]>) => {
                    if(profilesResult.success === true) {
                        console.log(profilesResult.data)
                        this.profile = profilesResult.data.find(
                            profile => profile.id === selectedID
                        )
                    }
                }
            );
        }
    }
}