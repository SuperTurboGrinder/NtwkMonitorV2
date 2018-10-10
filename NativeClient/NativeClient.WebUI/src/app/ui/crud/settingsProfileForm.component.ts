import { Component } from "@angular/core";
import { Location } from "@angular/common";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { SettingsProfile } from "../../model/httpModel/settingsProfile.model";
import { SettingsProfilesService } from "../../services/settingsProfiles.service";
import { HTTPResult } from "../../model/servicesModel/httpResult.model";
import { Range } from "../misc/numRangeSelector.component"

@Component({
    selector: 'settingsProfileForm',
    templateUrl: './settingsProfileForm.component.html'
})
export class SettingsProfileFormComponent {
    private _isEditMode: boolean = false;
    public original: SettingsProfile = null;
    public profile: SettingsProfile = null;
    public displayDiscardMessage = false;
    public displayOperationInProgress = false;

    public get isEditMode() {
        return this._isEditMode;
    }

    constructor(
        private location: Location,
        route: ActivatedRoute,
        private settingsService: SettingsProfilesService
    ) {
        this.profile = new SettingsProfile(
            0, "", 0, 24, true, true, 1
        );
        let routeSnapshot: ActivatedRouteSnapshot = route.snapshot;
        this._isEditMode = routeSnapshot.url[1].path === "edit";
        if(this._isEditMode) {
            let selectedID = parseInt(routeSnapshot.params.id);
            console.log(selectedID);
            settingsService.getProfiles().subscribe(
                (profilesResult: HTTPResult<SettingsProfile[]>) => {
                    if(profilesResult.success === true) {
                        this.original = profilesResult.data.find(
                            profile => profile.id === selectedID
                        );
                        this.profile = this.makeProfileCopy(this.original)
                    }
                }
            );
        } else {
            this.original = this.makeProfileCopy(this.profile);
        }
    }

    public setMonitorInterval(interval: number) {
        this.profile.monitorInterval = interval;
    }

    public updateMonitoringHoursRage(range: Range) {
        this.profile.monitoringStartHour = range.value;
        this.profile.monitoringSessionDuration = range.length;
    }

    private makeProfileCopy(profile: SettingsProfile): SettingsProfile {
        return new SettingsProfile(
            profile.id,
            profile.name,
            profile.monitoringStartHour,
            profile.monitoringSessionDuration,
            profile.startMonitoringOnLaunch,
            profile.depthMonitoring,
            profile.monitorInterval
        );
    }

    private isOkToDiscard(): boolean {
        return this.original.name === this.profile.name
            && this.original.monitoringStartHour === this.profile.monitoringStartHour
            && this.original.monitoringSessionDuration === this.profile.monitoringSessionDuration
            && this.original.startMonitoringOnLaunch === this.profile.startMonitoringOnLaunch
            && this.original.depthMonitoring === this.profile.depthMonitoring
            && this.original.monitorInterval === this.profile.monitorInterval
    }

    tryDiscard() {
        if(this.isOkToDiscard())
            this.discardAndReturn(true);
        else
            this.displayDiscardMessage = true;
    }

    discardAndReturn(shouldDiscard: boolean) {
        this.displayDiscardMessage = false;
        if(shouldDiscard) {
            this.location.back();
        }
    }

    tryCreate() {
        this.displayOperationInProgress = true;
        console.log(this.profile)
        this.settingsService.createNewProfile(
            this.profile,
            (success: boolean) => {
                if(success) {
                    //send success message
                    this.location.back();
                } else {
                    this.displayOperationInProgress = false;
                }
            }
        );
    }
}