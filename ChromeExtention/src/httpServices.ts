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
            `services/ping/${id}`,
            'GET',
            pingResult => {
                if (pingResult.isSuccess()) {
                    callback(pingResult.data);
                }
            }
        );
    }

    telnet(id: number) {
        this.httpClient.operationRequest(
            `services/telnet/${id}`,
            'GET',
            () => {}
        );
    }

    ssh(id: number) {
        this.httpClient.operationRequest(
            `services/ssh/${id}`,
            'GET',
            () => {}
        );
    }

    webService(nodeID: number, serviceID: number) {
        this.httpClient.operationRequest(
            `services/customWebService/${nodeID}/${serviceID}`,
            'GET',
            () => {}
        );
    }

    webServiceString(
        nodeID: number,
        serviceID: number,
        callback: (serviceUrl: string) => void
    ) {
        this.httpClient.rawDataRequest<string>(
            `services/customWebServiceString/${nodeID}/${serviceID}`,
            'GET',
            serviceStrResult => {
                if (serviceStrResult.isSuccess()) {
                    callback(serviceStrResult.data);
                }
            }
        );
    }
}