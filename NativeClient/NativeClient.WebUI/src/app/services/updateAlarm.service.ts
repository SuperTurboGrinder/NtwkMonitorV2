import { Injectable } from "@angular/core";
import { Observable, Subject } from "rxjs";

@Injectable()
export class UpdateAlarmService {
    private alarmSubjectNodesAndTags = new Subject<boolean>();
    
    public sendUpdateNodesAndTagsAlarm() {
        this.alarmSubjectNodesAndTags.next(true);
    }

    public get nodesAndTagsUpdateAlarm() : Observable<boolean> {
        return this.alarmSubjectNodesAndTags;
    }
}