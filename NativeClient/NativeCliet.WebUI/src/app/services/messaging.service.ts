import { Injectable } from '@angular/core';
import { Observable, Observer, Subject } from 'rxjs';

import { Messages } from "../model/servicesModel/messagesEnum.model"
import { BackendErrorStatuses } from '../model/httpModel/backendErrorStatuses.model'

@Injectable()
export class MessagingService {
    private messagesSubject = new Subject<Messages>();
    private backendErrorsSubject = new Subject<BackendErrorStatuses>();

    public reportBadRequestError(
        status: BackendErrorStatuses, 
        statusString: string
    ) {
        console.error(`Bad request error: ${statusString}.`);
        this.backendErrorsSubject.next(status);
    }

    public showMessage(message:Messages) {
        this.messagesSubject.next(message);
    }

    public get messages() : Observable<Messages> {
        return this.messagesSubject;
    }

    public get backendErrors() : Observable<BackendErrorStatuses> {
        return this.backendErrorsSubject;
    }
}