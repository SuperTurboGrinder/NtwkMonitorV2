import { Component } from "@angular/core";
import { Location } from "@angular/common";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { SettingsProfile } from "../../model/httpModel/settingsProfile.model";
import { SettingsProfilesService } from "../../services/settingsProfiles.service";
import { HTTPResult } from "../../model/servicesModel/httpResult.model";
import { Range } from "../misc/numRangeSelector.component"
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";
import { MessagingService } from "src/app/services/messaging.service";
import { BaseCrudFormComponent } from "../helpers/baseCrudFormComponent.helper";

@Component({
    selector: 'settingsProfileForm',
    templateUrl: './settingsProfileForm.component.html'
})
export class SettingsProfileFormComponent
    extends BaseCrudFormComponent<SettingsProfile, SettingsProfilesService> {

    constructor(
        messager: MessagingService,
        location: Location,
        route: ActivatedRoute,
        settingsService: SettingsProfilesService
    ) {
        super(messager, location, route, settingsService);
    }

    protected getOriginalData(
        id: number,
        callback: (success: boolean, orig: SettingsProfile) => void
    ) {
        this.dataService.getProfiles().subscribe(
            (profilesResult: HTTPResult<SettingsProfile[]>) => {
                callback(
                    profilesResult.success,
                    profilesResult.success
                        ? profilesResult.data.find(profile => profile.id === id)
                        : null
                );
            }
        );
    }

    protected newEmptyData(): SettingsProfile {
        return new SettingsProfile(
            0, "", 0, 24, true, true, 1
        )
    }

    protected currentIdenticalTo(obj: SettingsProfile): boolean {
        return obj.name === this.data.name
            && obj.monitoringStartHour === this.data.monitoringStartHour
            && obj.monitoringSessionDuration === this.data.monitoringSessionDuration
            && obj.startMonitoringOnLaunch === this.data.startMonitoringOnLaunch
            && obj.depthMonitoring === this.data.depthMonitoring
            && obj.monitorInterval === this.data.monitorInterval
    }

    protected makeCopy(orig: SettingsProfile) : SettingsProfile {
        return new SettingsProfile(
            orig.id,
            orig.name,
            orig.monitoringStartHour,
            orig.monitoringSessionDuration,
            orig.startMonitoringOnLaunch,
            orig.depthMonitoring,
            orig.monitorInterval
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

    public updateMonitoringHoursRage(range: Range) {
        this.data.monitoringStartHour = range.value;
        this.data.monitoringSessionDuration = range.length;
    }
}