import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SettingsProfile } from '../../model/httpModel/settingsProfile.model';
import { SettingsProfilesService } from '../../services/settingsProfiles.service';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { Range } from '../misc/numRangeSelector.component';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudFormComponent } from '../helpers/baseCrudFormComponent.helper';

@Component({
    selector: 'app-settings-profile-form',
    templateUrl: './settingsProfileForm.component.html'
})
export class SettingsProfileFormComponent
    extends BaseCrudFormComponent<SettingsProfile, SettingsProfilesService> {
    private _originalMonitorInterval: number = null;

    constructor(
        messager: MessagingService,
        location: Location,
        route: ActivatedRoute,
        settingsService: SettingsProfilesService
    ) {
        super(messager, location, route, settingsService);
    }

    public get originalMonitorInterval() {
        return this._originalMonitorInterval;
    }

    protected getOriginalData(
        id: number,
        callback: (success: boolean, orig: SettingsProfile) => void
    ) {
        this.dataService.getProfiles().subscribe(
            (profilesResult: HTTPResult<SettingsProfile[]>) => {
                const profile = profilesResult.success
                    ? profilesResult.data.find(p => p.id === id)
                    : null;
                if (profile !== null) {
                    this._originalMonitorInterval = profile.monitorInterval;
                }
                callback(
                    profilesResult.success,
                    profile
                );
            }
        );
    }

    protected newEmptyData(): SettingsProfile {
        return new SettingsProfile(
            0, '', true, true, 1, false
        );
    }

    protected currentIdenticalTo(obj: SettingsProfile): boolean {
        return obj.name === this.data.name
            && obj.startMonitoringOnLaunch === this.data.startMonitoringOnLaunch
            && obj.depthMonitoring === this.data.depthMonitoring
            && obj.monitorInterval === this.data.monitorInterval
            && obj.realTimePingUiUpdate === this.data.realTimePingUiUpdate;
    }

    protected makeCopy(orig: SettingsProfile): SettingsProfile {
        return new SettingsProfile(
            orig.id,
            orig.name,
            orig.startMonitoringOnLaunch,
            orig.depthMonitoring,
            orig.monitorInterval,
            orig.realTimePingUiUpdate
        );
    }

    protected saveAsNewObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.createNewProfile(
            this.data,
            callback
        );
    }

    protected saveChangesToObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.updateProfile(
            this.data,
            callback
        );
    }

    public setMonitorInterval(interval: number) {
        this.data.monitorInterval = interval;
    }
}
