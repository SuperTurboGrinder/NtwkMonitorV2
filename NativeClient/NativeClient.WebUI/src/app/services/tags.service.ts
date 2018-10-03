import { Injectable, Inject } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

import { NodeTag } from "../model/httpModel/nodeTag.model";
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { UpdateAlarmService } from './updateAlarm.service';
import { BaseURL } from "./baseUrl.token";

@Injectable()
export class TagsService {
    private tagsListSubject = new BehaviorSubject<HTTPResult<NodeTag[]>>(null);
    private baseUrl: string = null;

    constructor(
        private updateAlarmService: UpdateAlarmService,
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'nodeTags';
        updateAlarmService.nodesAndTagsUpdateAlarm.subscribe(_ =>
            this.updateTags()
        );
    }

    public getTagsList() : Observable<HTTPResult<NodeTag[]>> {
        if(this.tagsListSubject.value == null) {
            this.updateAlarmService.sendUpdateNodesAndTagsAlarm();
        }
        return this.tagsListSubject;
    }

    private updateTags() {
        this.httpDatasource.dataRequest<NodeTag[]>(
            'get',
            this.baseUrl
        ).subscribe(result => this.tagsListSubject.next(result));
    }
}