import { Injectable, Inject } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

import { NtwkNode } from '../model/httpModel/ntwkNode.model';
import { CWSData } from "../model/httpModel/cwsData.model";
import { AllNodesData } from '../model/httpModel/allNodesData.model';
import { NtwkNodesTree } from '../model/viewModel/ntwkNodesTree.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { UpdateAlarmService } from './updateAlarm.service';
import { BaseURL } from "./baseUrl.token";

@Injectable()
export class NodesService {
    private nodesTreeSubject : BehaviorSubject<HTTPResult<{
        cwsData: CWSData[],
        nodesTree: NtwkNodesTree
    }>> = null;
    private baseUrl: string = null;

    constructor(
        private updateAlarmService: UpdateAlarmService,
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'nodes';
        updateAlarmService.nodesAndTagsUpdateAlarm.subscribe(_ =>
            this.updateNodesTree()
        );
    }

    public getNodesTree() : Observable<HTTPResult<{
        cwsData: CWSData[],
        nodesTree: NtwkNodesTree
    }>> {
        this.checkIfUpdateNeeded();
        return this.nodesTreeSubject;
    }

    private checkIfUpdateNeeded() {
        if(this.nodesTreeSubject == null) {
            this.nodesTreeSubject =
                new BehaviorSubject<HTTPResult<{
                    cwsData: CWSData[],
                    nodesTree: NtwkNodesTree
                }>>(null);
            this.updateAlarmService.sendUpdateNodesAndTagsAlarm();
        }
    }

    public createNode(node: NtwkNode) : Observable<HTTPResult<NtwkNode>> {
        var result = this.httpDatasource.dataRequest<NtwkNode>(
            'get',
            this.baseUrl+'/new',
            node
        );
        result.subscribe(result => {
            if(result.success === true) {
                this.updateAlarmService.sendUpdateNodesAndTagsAlarm();
            }
        });
        return result;
    }

    private updateNodesTree() {
        this.httpDatasource.dataRequest<AllNodesData>(
            'get',
            this.baseUrl
        ).subscribe(result => 
            this.nodesTreeSubject.next(result.success === true
                ? HTTPResult.Successful({
                    cwsData: result.data.webServicesData,
                    nodesTree: new NtwkNodesTree(result.data)
                })
                : HTTPResult.Failed()
            )
        );
    }
}