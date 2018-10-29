import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, retry, map } from 'rxjs/operators';

import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { MessagingService } from './messaging.service';
import { BackendErrorStatuses } from '../model/httpModel/backendErrorStatuses.model';

@Injectable()
export class HTTPDatasource {
    constructor(
        private httpClient: HttpClient,
        private messager: MessagingService
    ) {}

    public dataRequest<T>(method: string, url: string, body?: T): Observable<HTTPResult<T>> {
        return this.checkedData(
            this.httpClient.request<T>(method, url, {
                body: body,
                observe: 'response',
                responseType: 'json'
            })
        );
    }

    public operationRequest(method: string, url: string): Observable<boolean> {
        return this.checkedOperation(
            this.httpClient.request(method, url, {
                observe: 'response',
                responseType: 'text'
            })
        );
    }

    public dataOperationRequest<T>(method: string, url: string, body: T): Observable<boolean> {
        return this.checkedOperation(
            this.httpClient.request(method, url, {
                body: body,
                observe: 'response',
                responseType: 'text'
            })
        );
    }

    private checkedData<T>(result: Observable<HttpResponse<T>>): Observable<HTTPResult<T>> {
        return result.pipe(
            retry(2),
            map((response: HttpResponse<T>) => {
                if (response.status === 204) {
                    throw new Error('Wrong response (204) when expecting 200.');
                }
                return HTTPResult.Successful(response.body);
            }),
            catchError(err => {
                this.handleError(err);
                return of(HTTPResult.Failed<T>());
            })
        );
    }

    private checkedOperation<T>(result: Observable<HttpResponse<any>>): Observable<boolean> {
        return result.pipe(
            map((response: HttpResponse<T>) => {
                if (response.status === 200) {
                    throw new Error('Wrong response (200) when expecting 204.');
                }
                return true;
            }),
            catchError(err => {
                this.handleError(err);
                return of(false);
            })
        );
    }

    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
            console.error('An error occurred:', error.error.message);
        } else {
            if (error.status === 500) {
                console.error('Backend returned code 500.');
            } else if (error.status === 400) {
                const badRequestData: {
                    status: BackendErrorStatuses,
                    statusString: string
                } = (error.error.constructor === ''.constructor)
                    ? JSON.parse(error.error)
                    : error.error;
                this.messager.reportBadRequestError(
                    badRequestData.status,
                    badRequestData.statusString
                );
            } else if (error.status === 0) {
                console.error('Backend server not found. Check if it is running.');
            } else {
                console.error(
                    `Backend returned code ${error.status}, ` +
                    `body was: ${error.error}`
                );
            }
        }
    }
}
