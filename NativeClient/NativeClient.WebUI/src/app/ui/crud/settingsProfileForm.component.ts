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
    private originalSessionDurationRange: Range = null;

    public displayOperationInProgress = false;
    public displayDiscardMessage = false;
    public displayCreateConfirmationMessage = false;
    public displaySaveChangesMessage = false;

    public get isEditMode() {
        return this._isEditMode;
    }

    public get originalMonitorSessionRange(): Range {
        if(this.original === null) return null;
        if(this.originalSessionDurationRange === null)
            this.originalSessionDurationRange = new Range(
                this.original.monitoringStartHour,
                this.original.monitoringSessionDuration
            );
        return this.originalSessionDurationRange;
    }

    public get originalMonitorInterval(): number {
        if(this.original === null) return null;
        return this.original.monitorInterval;
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

    public tryDiscard() {
        if(this.isOkToDiscard())
            this.discardAndReturn(true);
        else
            this.displayDiscardMessage = true;
    }

    public discardAndReturn(shouldDiscard: boolean) {
        this.displayDiscardMessage = false;
        if(shouldDiscard) {
            this.location.back();
        }
    }

    private confirmOperationSuccess(success: boolean) {
        if(success) {
            //send success message
            this.location.back();
        } else {
            this.displayOperationInProgress = false;
        }
    }

    public tryCreateNew() {
        this.displayCreateConfirmationMessage = true;
    }

    public createNewProfileAndReturn(shouldCreate: boolean) {
        this.displayCreateConfirmationMessage = false;
        if(!shouldCreate) return;
        this.displayOperationInProgress = true;
        this.settingsService.createNewProfile(
            this.profile,
            (success: boolean) => this.confirmOperationSuccess(success)
        );
    }

    public trySaveChanges() {
        if(this.isOkToDiscard())
            { /*send no changes detected message*/ }
        else
            this.displaySaveChangesMessage = true;
    }

    public saveChangesToProfileAndReturn(shouldSave: boolean) {
        this.displaySaveChangesMessage = false;
        if(!shouldSave) return;
        this.displayOperationInProgress = true;
        this.settingsService.updateProfile(
            this.profile,
            (success: boolean) => this.confirmOperationSuccess(success)
        );
    }
}