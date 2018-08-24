import { Injectable, Inject } from "@angular/core";
import { Observable, Subject } from "rxjs";
import { map } from "rxjs/operators";

import { NtwkNode } from '../model/httpModel/ntwkNode.model';
import { AllNodesData } from '../model/httpModel/allNodesData.model';
import { NtwkNodesTree } from '../model/viewModel/ntwkNodesTree.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { UpdateAlarmService } from './updateAlarm.service';
import { BaseURL } from "./baseUrl.token";

@Injectable()
export class NodesRepository {
    private nodesTreeSubject : Subject<NtwkNodesTree> = null;
    private baseUrl: string = null;

    constructor(
        private updateAlarmService: UpdateAlarmService,
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'nodes';
        updateAlarmService.alarm.subscribe(_ =>
            this.updateNodesTree()
        );
    }

    public getNodesTree() : Observable<NtwkNodesTree> {
        if(this.nodesTreeSubject == null) {
            this.nodesTreeSubject = new Subject<NtwkNodesTree>();
            this.updateAlarmService.sendAlarm();
        }
        return this.nodesTreeSubject;
    }

    public createNode(node: NtwkNode) : Observable<HTTPResult<NtwkNode>> {
        var result = this.httpDatasource.post<NtwkNode>(
            this.baseUrl+'/new',
            node
        );
        result.subscribe(result => {
            if(result.success === true) {
                this.updateAlarmService.sendAlarm();
            }
        });
        return result;
    }

    private alarmWhenSuccessful<T>(data: Observable<T>) : Observable<T> {
        data.subscribe(n => {
            if(n != null) {
                this.updateAlarmService.sendAlarm();
            }
        });
        return data;
    }

    private updateNodesTree() {
        this.httpDatasource.get<AllNodesData>(this.baseUrl)
            .subscribe(result => {
                if(result.success === true) {
                    this.nodesTreeSubject.next(new NtwkNodesTree(result.data))
                }
            });
    }
}