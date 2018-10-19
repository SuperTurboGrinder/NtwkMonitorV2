import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { NtwkNode } from '../model/httpModel/ntwkNode.model';
import { CWSData } from '../model/httpModel/cwsData.model';
import { AllNodesData } from '../model/httpModel/allNodesData.model';
import { NtwkNodesTree } from '../model/viewModel/ntwkNodesTree.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { BaseURL } from './baseUrl.token';

@Injectable()
export class NodesService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'nodes';
    }

    public getNodesTree(): Observable<HTTPResult<{
        cwsData: CWSData[],
        nodesTree: NtwkNodesTree
    }>> {
        return this.httpDatasource.dataRequest<AllNodesData>(
            'get',
            this.baseUrl
        ).pipe(
            map(result =>
                result.success === true
                    ? HTTPResult.Successful({
                        cwsData: result.data.webServicesData,
                        nodesTree: new NtwkNodesTree(result.data)
                    })
                    : HTTPResult.Failed()
            )
        );
    }

    public createNode(node: NtwkNode): Observable<HTTPResult<NtwkNode>> {
        return this.httpDatasource.dataRequest<NtwkNode>(
            'get',
            this.baseUrl + '/new',
            node
        );
    }
}
