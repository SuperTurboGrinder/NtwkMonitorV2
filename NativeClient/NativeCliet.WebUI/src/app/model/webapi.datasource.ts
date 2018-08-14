import { Injectable } from "@angular/core";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Observable, of } from "rxjs";
import { catchError, retry } from "rxjs/operators";

@Injectable()
export class WebAPIDatasource {
    constructor(
        private httpClient: HttpClient
    ) {}

    public get<T>(url:string): Observable<T> {
        return this.httpClient.get<T>(url).pipe(
            retry(5),
            catchError(err => {
                this.handleError(err);
                return of(null);
            })
        );
    }

    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
            console.error('An error occurred:', error.error.message);
        } else {
            if(error.status == 500) {
                console.error("Backend operation failed. Please report this error.")
            } else {
                console.error(
                    `Backend returned code ${error.status}, ` +
                    `body was: ${error.error}`
                );
            }
        }
    }
}