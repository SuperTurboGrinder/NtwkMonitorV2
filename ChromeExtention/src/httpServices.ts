import { CWSData, NodeData, AllNodesData, PingTestData } from "./model";
import { ExtHttpClient } from "./baseHttp";

export class HttpDatasource {
    constructor(
        private httpClient: ExtHttpClient
    ) {}

    /*
    private getTagsList(callback: (tags: NodeTag[]) => void) {
        this.httpClient.dataRequest<NodeTag[]>('nodeTags', 'GET', result => {
            if (result.isSuccess()) {
                callback(result.data);
            }
        });
    }
    */
    
    getAllNodesData(
        callback: (
            nodeDataLayers: NodeData[][],
            wsData: CWSData[],
            // tagsList: NodeTag[]
        ) => void
    ) {
        this.httpClient.dataRequest<AllNodesData>('nodes', 'GET', result => {
            if (result.isSuccess()) {
                const wsData = result.data.webServicesData;
                const nodesDataLayers = result.data.nodesData;
                // this.getTagsList(tagsList => 
                    callback(nodesDataLayers, wsData, /*tagsList*/);
                // );
            }
        });
    }
}

export class HttpNodeServices {
    constructor(
        private httpClient: ExtHttpClient
    ) {}

    ping(
        id: number,
        callback: (pingResult: PingTestData) => void
    ) {
        this.httpClient.dataRequest<PingTestData>(
            'GET',
            `services/ping/${id}`,
            pingResult => {
                if (pingResult.isSuccess()) {
                    callback(pingResult.data);
                }
            }
        );
    }

    telnet(id: number) {
        this.httpClient.operationRequest(
            'GET',
            `api/services/telnet/${id}`,
            () => {}
        );
    }

    ssh(id: number) {
        this.httpClient.operationRequest(
            'GET',
            `api/services/ssh/${id}`,
            () => {}
        );
    }

    webService(nodeID: number, serviceID: number) {
        this.httpClient.operationRequest(
            'GET',
            `api/services/customWebService/${nodeID}/${serviceID}`,
            () => {}
        );
    }

    webServiceString(
        nodeID: number,
        serviceID: number,
        callback: (serviceUrl: string) => void
    ) {
        this.httpClient.dataRequest<string>(
            'GET',
            `api/services/customWebServiceString/${nodeID}/${serviceID}`,
            serviceStrResult => {
                if (serviceStrResult.isSuccess()) {
                    callback(serviceStrResult.data);
                }
            }
        );
    }
}