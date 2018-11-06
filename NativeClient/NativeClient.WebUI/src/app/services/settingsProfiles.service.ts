import { Injectable, Inject } from '@angular/core';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, first, switchMap, concatMap } from 'rxjs/operators';

import { SettingsProfile } from '../model/httpModel/settingsProfile.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { BaseURL } from './baseUrl.token';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';

@Injectable()
export class SettingsProfilesService {
    private currentProfileID = -1; // not assigned
    private currentProfileSubject: BehaviorSubject<SettingsProfile> = null;
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'settingsProfiles';
    }

    public getProfiles(): Observable<HTTPResult<SettingsProfile[]>> {
        return this.httpDatasource.dataRequest<SettingsProfile[]>(
            'get',
            this.baseUrl
        );
    }

    public isCurrentProfileSet() {
        return this.currentProfileID !== -1;
    }

    public setCurrentProfile(id: number): Observable<SettingsProfile> {
        this.currentProfileID = id;
        return this.getProfiles().pipe(
            switchMap(results => {
                if (results.success) {
                    const result = results.data.find(p => p.id === id);
                    if (this.currentProfileSubject == null) {
                        this.currentProfileSubject =
                            new BehaviorSubject<SettingsProfile>(result);
                    } else {
                        this.currentProfileSubject.next(result);
                    }
                }
                return this.currentProfileSubject;
            })
        );
    }

    public isCurrentProfileID(id: number): boolean {
        return this.currentProfileID === id;
    }

    public currentProfilesViewTagFilterData(): Observable<HTTPResult<TagFilterData>> {
        return this.httpDatasource.dataRequest<TagFilterData>(
            'get',
            this.baseUrl + `/${this.currentProfileID}/mainViewTagFilterData`
        );
    }

    public currentProfilesMonitorTagFilterData(): Observable<HTTPResult<TagFilterData>> {
        return this.httpDatasource.dataRequest<TagFilterData>(
            'get',
            this.baseUrl + `/${this.currentProfileID}/monitorTagFilterData`
        );
    }

    private pipeThroughCurrentProfileUpdateOnTrue(
        orig: Observable<boolean>
    ): Observable<boolean> {
        return orig.pipe(
            concatMap(success => success
                ? this.setCurrentProfile(this.currentProfileID)
                        .pipe(map(pr => success), first())
                : of(success)
            )
        );
    }

    private updateCurrentProfileTagSet(
        urlSuffix: string,
        newTagsIDs: number[]
    ): Observable<boolean> {
        return this.pipeThroughCurrentProfileUpdateOnTrue(
                this.httpDatasource.dataOperationRequest(
                'post',
                this.baseUrl + `/${this.currentProfileID}/${urlSuffix}`,
                newTagsIDs
            )
        );
    }

    public setCurrentProfilesViewTags(newTagsIDs: number[]): Observable<boolean> {
        return this.updateCurrentProfileTagSet(
            'setViewTags',
            newTagsIDs
        );
    }

    public setCurrentProfilesMonitorTags(newTagsIDs: number[]): Observable<boolean> {
        return this.updateCurrentProfileTagSet(
            'setMonitorTags',
            newTagsIDs
        );
    }

    public setCurrentProfilesViewTagsToMonitorTags() {
        return this.pipeThroughCurrentProfileUpdateOnTrue(
                this.httpDatasource.operationRequest(
                'post',
                this.baseUrl + `/${this.currentProfileID}/setViewTagsToMonitorTags`
            )
        );
    }

    public setCurrentProfilesMonitorTagsToViewTags() {
        return this.pipeThroughCurrentProfileUpdateOnTrue(
                this.httpDatasource.operationRequest(
                'post',
                this.baseUrl + `/${this.currentProfileID}/setMonitorTagsToViewTags`
            )
        );
    }

    public createNewProfile(
        newProfile: SettingsProfile,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'post',
            this.baseUrl + `/new`,
            newProfile
        ).subscribe(
            (result: HTTPResult<SettingsProfile>) => callback(result.success)
        );
    }

    public updateProfile(
        newProfileState: SettingsProfile,
        callback: (success: boolean) => void
    ) {
        return  this.pipeThroughCurrentProfileUpdateOnTrue(
            this.httpDatasource.dataOperationRequest(
                'put',
                this.baseUrl + `/${newProfileState.id}/update`,
                newProfileState
            )
        ).subscribe(
            callback
        );
    }

    public deleteProfile(
        id: number,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'delete',
            this.baseUrl + `/${id}/remove`
        ).subscribe(
            (result: HTTPResult<SettingsProfile>) => callback(result.success)
        );
    }
}
