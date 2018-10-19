import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';

import { CustomWebService } from '../model/httpModel/customWebService.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { BaseURL } from './baseUrl.token';
import { CustomWebServiceBinding } from '../model/httpModel/cwsBindingData.model';

@Injectable()
export class CustomWebServicesService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'webServices';
    }

    public getCWSList(): Observable<HTTPResult<CustomWebService[]>> {
        return this.httpDatasource.dataRequest<CustomWebService[]>(
            'get',
            this.baseUrl
        );
    }

    public createNewCWS(
        newCWS: CustomWebService,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'post',
            this.baseUrl + `/new`,
            newCWS
        ).subscribe(
            (result: HTTPResult<CustomWebService>) => callback(result.success)
        );
    }

    public updateCWS(
        newCWSState: CustomWebService,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataOperationRequest(
            'put',
            this.baseUrl + `/${newCWSState.id}/update`,
            newCWSState
        ).subscribe(
            (success: boolean) => callback(success)
        );
    }

    public deleteCWS(
        id: number,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'delete',
            this.baseUrl + `/${id}/delete`
        ).subscribe(
            (result: HTTPResult<CustomWebService>) => callback(result.success)
        );
    }

    public createNewCWSBinding(
        serviceID: number,
        newCWSBindingData: CustomWebServiceBinding,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataOperationRequest(
            'post',
            this.baseUrl + `/${serviceID}/bind`,
            newCWSBindingData
        ).subscribe(
            callback
        );
    }

    public updateCWSBinding(
        serviceID: number,
        newCWSBindingState: CustomWebServiceBinding,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataOperationRequest(
            'put',
            this.baseUrl + `/${serviceID}/updateBinding`,
            newCWSBindingState
        ).subscribe(
            callback
        );
    }

    public deleteCWSBinding(
        serviceID: number,
        nodeID: number,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.operationRequest(
            'delete',
            this.baseUrl + `/${serviceID}/deleteBindingWithNode/${nodeID}`
        ).subscribe(
            callback
        );
    }
}
