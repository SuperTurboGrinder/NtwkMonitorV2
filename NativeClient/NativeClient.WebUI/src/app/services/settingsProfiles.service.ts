import { Injectable, Inject } from '@angular/core';
import { Observable, BehaviorSubject, of, Subscription, forkJoin } from 'rxjs';
import { map, first, switchMap, concatMap, take } from 'rxjs/operators';

import { SettingsProfile } from '../model/httpModel/settingsProfile.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { BaseURL } from './baseUrl.token';
import { TagFilterData } from '../model/httpModel/tagFilterData.model';

class CurrentProfileData {
    profile: SettingsProfile;
    viewTagFilter: TagFilterData;
    monitorTagFilter: TagFilterData;
}

@Injectable()
export class SettingsProfilesService {
    private currentProfileID = -1; // not assigned
    private currentProfileDataSubject:
        BehaviorSubject<CurrentProfileData> = null;
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

    public subscribeToSettingsChange(
        callback: (settings: {
            profile: SettingsProfile,
            viewTagFilter: TagFilterData,
            monitorTagFilter: TagFilterData
        }) => void
    ): Subscription {
        return this.currentProfileDataSubject.subscribe(callback);
    }

    public isCurrentProfileSet() {
        return this.currentProfileID !== -1;
    }

    public setCurrentProfile(id: number): Observable<CurrentProfileData> {
        this.currentProfileID = id;
        const newCurrentProfile = this.getProfiles().pipe(
            map(
                results => results.success === true
                    ? HTTPResult.Successful(
                        results.data.find(p => p.id === id)
                    )
                    : HTTPResult.Failed<SettingsProfile>()
            )
        );
        const newCurrentPrifileViewFilterData =
            this.httpDatasource.dataRequest<TagFilterData>(
                'get',
                this.baseUrl + `/${this.currentProfileID}/mainViewTagFilterData`
            );
        const newCurrentProfileMonitorFilterData =
            this.httpDatasource.dataRequest<TagFilterData>(
                'get',
                this.baseUrl + `/${this.currentProfileID}/monitorTagFilterData`
            );
        return forkJoin(
            newCurrentProfile,
            newCurrentPrifileViewFilterData,
            newCurrentProfileMonitorFilterData
        ).pipe(switchMap(results => {
            if (results[0].success === false
            || results[1].success === false
            || results[2].success === false) {
                return;
            }
            const data = ({
                profile:  results[0].data,
                viewTagFilter:  results[1].data,
                monitorTagFilter: results[2].data
            });
            if (this.currentProfileDataSubject == null) {
                this.currentProfileDataSubject =
                    new BehaviorSubject(data);
            } else {
                this.currentProfileDataSubject.next(data);
            }
            return this.currentProfileDataSubject;
        }));
    }

    public isCurrentProfileID(id: number): boolean {
        return this.currentProfileID === id;
    }

    public currentProfilesViewTagFilterData(): Observable<TagFilterData> {
        return this.currentProfileDataSubject.asObservable().pipe(
            map(data => data.viewTagFilter),
            take(1)
        );
    }

    public currentProfilesMonitorTagFilterData(): Observable<TagFilterData> {
        return this.currentProfileDataSubject.asObservable().pipe(
            map(data => data.monitorTagFilter),
            take(1)
        );
    }

    private pipeThroughCurrentProfileUpdateOnTrue(
        orig: Observable<boolean>
    ): Observable<boolean> {
        return orig.pipe(
            concatMap(success => success === true
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
