import { ExtHttpClient } from "./baseHttp";
import { HttpDatasource, HttpNodeServices } from "./httpServices";
import { NodesTreeMap } from "./nodesPacking";
import { Implementation } from "./impl";

function main() {
    const httpClient = new ExtHttpClient();
    const httpDatasource = new HttpDatasource(httpClient);
    const httpNodeServices = new HttpNodeServices(httpClient);

    httpDatasource.getAllNodesData(
        (nodeDataLayers, wsData) => {
            const nodesMap = new NodesTreeMap(nodeDataLayers, wsData, httpNodeServices);
            const impl = new Implementation();
            const requestedNodes = nodesMap
                .getContainersByNames(impl.nodesNames);

        }
    );
}

main();