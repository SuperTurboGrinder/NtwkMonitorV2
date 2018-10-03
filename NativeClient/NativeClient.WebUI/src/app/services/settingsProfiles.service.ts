import { Injectable, Inject } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";
import { map, switchMap } from "rxjs/operators";

import { SettingsProfile } from '../model/httpModel/settingsProfile.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { BaseURL } from "./baseUrl.token";

@Injectable()
export class SettingsProfilesService {
    private currentProfileID = -1; //not assigned
    private currentProfileSubject : BehaviorSubject<SettingsProfile> = null;
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'settingsProfiles';
    }

    public getProfiles() : Observable<HTTPResult<SettingsProfile[]>> {
        return this.httpDatasource.dataRequest<SettingsProfile[]>(
            'get',
            this.baseUrl
        );
    }

    public isCurrentProfileSet() {
        return this.currentProfileID !== -1;
    }

    public setCurrentProfile(id: number) : Observable<SettingsProfile> {
        this.currentProfileID = id;
        return this.getProfiles().pipe(
            switchMap(results => {
                if(results.success) {
                    let result = results.data.find(p => p.id == id);
                    if(this.currentProfileSubject == null) {
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

    public get currentProfile() : Observable<SettingsProfile> {
        return this.currentProfileSubject;
    }

    public currentProfilesViewNodesIDs() : Observable<HTTPResult<number[]>> {
        return this.httpDatasource.dataRequest<number[]>(
            'get',
            this.baseUrl+`/${this.currentProfileID}/mainViewNodesIDs`
        );
    }
}