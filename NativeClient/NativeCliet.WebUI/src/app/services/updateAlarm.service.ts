import { Injectable } from "@angular/core";
import { Observable, Subject } from "rxjs";

@Injectable()
export class UpdateAlarmService {
    private alarmSubject = new Subject<boolean>();
    
    public sendAlarm() {
        this.alarmSubject.next(true);
    }

    public get alarm() : Observable<boolean> {
        return this.alarmSubject;
    }
}