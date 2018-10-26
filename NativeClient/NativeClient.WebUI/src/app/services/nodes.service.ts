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

    public setNodeTags(
        nodeID: number,
        tagsIDs: number[],
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataOperationRequest(
            'put',
            this.baseUrl + `/${nodeID}/setTags`,
            tagsIDs
        ).subscribe(
            callback
        );
    }

    public createNewNode(
        node: NtwkNode,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'post',
            this.baseUrl + `/new`,
            node
        ).subscribe(
            (result: HTTPResult<NtwkNode>) => callback(result.success)
        );
    }

    public createNewNodeWithParent(
        node: NtwkNode,
        parentID: number,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'post',
            this.baseUrl + `/${parentID}/new`,
            node
        ).subscribe(
            (result: HTTPResult<NtwkNode>) => callback(result.success)
        );
    }

    public moveNodeToNewParent(
        nodeID: number,
        newParentID: number,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.operationRequest(
            'put',
            this.baseUrl + `/${nodeID}/changeParentTo/${newParentID}`
        ).subscribe(
            callback
        );
    }

    public updateNode(
        newNodeState: NtwkNode,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataOperationRequest(
            'put',
            this.baseUrl + `/${newNodeState.id}/update`,
            newNodeState
        ).subscribe(
            callback
        );
    }

    public deleteNode(
        id: number,
        callback: (success: boolean) => void
    ) {
        return this.httpDatasource.dataRequest(
            'delete',
            this.baseUrl + `/${id}/delete`
        ).subscribe(
            (result: HTTPResult<NtwkNode>) => callback(result.success)
        );
    }
}
