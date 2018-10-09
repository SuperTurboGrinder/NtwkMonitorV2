import { Injectable, Inject } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";

import { NodeTag } from "../model/httpModel/nodeTag.model";
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { BaseURL } from "./baseUrl.token";

@Injectable()
export class TagsService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'nodeTags';
    }

    public getTagsList() : Observable<HTTPResult<NodeTag[]>> {
        return this.httpDatasource.dataRequest<NodeTag[]>(
            'get',
            this.baseUrl
        );
    }
}