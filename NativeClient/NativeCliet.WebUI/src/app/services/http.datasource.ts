import { Injectable } from "@angular/core";
import { HttpClient, HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { Observable, of } from "rxjs";
import { catchError, retry, map } from "rxjs/operators";

import { HTTPResult } from "../model/servicesModel/httpResult.model";
import { MessagingService } from "./messaging.service"

@Injectable()
export class HTTPDatasource {
    constructor(
        private httpClient: HttpClient,
        private messager: MessagingService
    ) {}

    public get<T>(url:string): Observable<HTTPResult<T>> {
        return this.checkedData(this.httpClient.get<HttpResponse<T>>(url))
    }

    public post<T>(url:string, body:T): Observable<HTTPResult<T>> {
        return this.checkedData(this.httpClient.post<HttpResponse<T>>(url, body))
    }

    public put<T>(url:string, body:T): Observable<HTTPResult<T>> {
        return this.checkedData(this.httpClient.put<HttpResponse<T>>(url, body))
    }

    public delete<T>(url:string): Observable<HTTPResult<T>> {
        return this.checkedData(this.httpClient.delete<HttpResponse<T>>(url));
    }

    private checkedData<T>(result: Observable<HttpResponse<T>>) : Observable<HTTPResult<T>> {
        return result.pipe(
            retry(5),
            map((response: HttpResponse<T>) => {
                if(response.status == 204) {
                    throw new Error("Wrong response (204) when expecting 200.");
                }
                return HTTPResult.Successful(response.body);
            }),
            catchError(err => {
                this.handleError(err);
                return of(HTTPResult.Failed<T>());
            })
        );
    }

    private handleError(error: HttpErrorResponse) {
        console.log("ERROR_HANDLING");
        if (error.error instanceof ErrorEvent) {
            console.error('An error occurred:', error.error.message);
        } else {
            if(error.status == 500) {
                console.error("Backend returned code 500.")
            } else if(error.status == 400) {
                var badRequestData = JSON.parse(error.error);
                this.messager.reportBadRequestError(
                    badRequestData.status,
                    badRequestData.statusString
                );
            } else {
                console.error(
                    `Backend returned code ${error.status}, ` +
                    `body was: ${error.error}`
                );
            }
        }
    }
}