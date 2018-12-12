import { NodeData, CWSData, PingTestData } from "./model";
import { HttpNodeServices } from "./httpServices";

class NodeCoord {
    constructor(
        readonly thisCoordIndex: number,
        readonly index: number,
        readonly layer: number,
        readonly id: number,
        readonly parentID: number,
        readonly name: string
    ) {}
}

export class SmartNodeContainer {
    private cwsNamesCache: string[] = null;
    private searchedForParent = false;
    private searchedForChildren = false;
    private _parent: SmartNodeContainer = null;
    private _children: SmartNodeContainer[] = [];

    constructor(
        private treeMap: NodesTreeMap,
        private readonly nodeCoordIndex: number,
        private readonly nodeData: NodeData,
        private readonly cwsData: CWSData[],
        private readonly httpNodeServices: HttpNodeServices
    ) {}
    
    get webServicesNames(): string[] {
        if (this.cwsNamesCache === null) {
            this.cwsNamesCache = this.cwsData.map(cwsd => cwsd.name);
        }
        return this.cwsNamesCache;
    }

    get parent(): SmartNodeContainer {
        if (this.searchedForParent) {
            this._parent = this.treeMap
                .getContainerParent(this.nodeCoordIndex);
        }
        return this._parent;
    }

    get children(): SmartNodeContainer[] {
        if (this.searchedForChildren) {
            this._children = this.treeMap
                .getContainersChildren(this.nodeCoordIndex);
        }
        return this._children;
    }

    get hasTelnet(): boolean {
        return this.nodeData.node.isOpenTelnet;
    }

    get hasSSH(): boolean {
        return this.nodeData.node.isOpenSSH;
    }

    get hasPing(): boolean {
        return this.nodeData.node.isOpenPing;
    }

    openTelnet() {
        if (!this.hasTelnet) {
            throw new Error(`openTelnet(): ${this.nodeData.node.name} has no telnet service`);
        }
        this.httpNodeServices.telnet(this.nodeData.node.id);
    }

    openSSH() {
        if (!this.hasSSH) {
            throw new Error(`openSSH(): ${this.nodeData.node.name} has no ssh service`);
        }
        this.httpNodeServices.ssh(this.nodeData.node.id);
    }
    
    getPing(callback: (testData: PingTestData) => void) {
        if (!this.hasSSH) {
            throw new Error(`openPing(): ${this.nodeData.node.name} has no ping service`);
        }
        this.httpNodeServices.ping(this.nodeData.node.id, callback);
    }

    openWebService(name: string) {
        const cws = this.cwsByName(name);
        this.httpNodeServices.webService(this.nodeData.node.id, cws.id);
    }

    getWebServiceString(
        name: string,
        callback: (serviceUrl: string) => void
    ) {
        const cws = this.cwsByName(name);
        this.httpNodeServices
            .webServiceString(this.nodeData.node.id, cws.id, callback);
    }

    private cwsByName(name: string) {
        const cws = this.cwsData.find(ws => ws.name === name);
        if (cws === undefined) {
            throw new Error(`openWebService(): ${this.nodeData.node.name} does not have ${name} service`);
        }
        return cws;
    }
}

export class NodesTreeMap {
    private containersMap: SmartNodeContainer[][] = null;
    private nodesCoords: NodeCoord[];

    constructor(
        private nodeDataLayers: NodeData[][],
        private wsData: CWSData[],
        private httpNodeServices: HttpNodeServices
    ) {
        this.containersMap = nodeDataLayers.reduce(
            (prev, current) => {
                prev.push(
                    new Array<SmartNodeContainer>(current.length)
                );
                return prev;
            },
            new Array<SmartNodeContainer[]>()
        );
        this.buildCoords();
    }

    getContainersByNames(names: string[]): SmartNodeContainer[] {
        return names.map(name => this.findContainer(c => c.name === name));
    }

    getContainerParent(coordIndex: number) {
        const parentID = this.nodesCoords[coordIndex].parentID;
        if (parentID === null) {
            return null;
        }
        return this.findContainer(c => c.id === parentID);
    }

    getContainersChildren(coordIndex: number) {
        const coord = this.nodesCoords[coordIndex];
        return this.nodesCoords.filter(
            c => c.parentID === coord.id
        ).map(c => this.getContainerByCoordIndex(c.thisCoordIndex));
    }

    private findContainer(
        pred: (coord: NodeCoord) => boolean
    ): SmartNodeContainer {
        const coordIndex = this.nodesCoords.findIndex(pred);
        if (coordIndex === -1) {
            return null;
        }
        return this.getContainerByCoordIndex(coordIndex);
    }

    private buildCoords() {
        let coordIndex = 0;
        this.nodesCoords = this.nodeDataLayers.map((layer, layerIndex) =>
            layer.map((nd, index) => new NodeCoord(
                coordIndex++,
                index,
                layerIndex,
                nd.node.id,
                nd.node.parentID,
                nd.node.name
            ))
        ).reduce((prev, next) => prev.concat(next), []);
    }

    private getContainerByCoordIndex(coordIndex: number): SmartNodeContainer {
        const coord = this.nodesCoords[coordIndex];
        const initial = this.containersMap[coord.layer][coord.index];
        if (initial) {
            return initial;
        }
        return this.buildContainer(coordIndex, coord);
    }

    private buildContainer(coordIndex: number, coord: NodeCoord) {
        const data = this.nodeDataLayers[coord.layer][coord.index];
        const container =
            new SmartNodeContainer(
                this,
                coordIndex,
                data,
                this.wsData.filter(
                    wsd => data.boundWebServicesIDs.includes(wsd.id)
                ),
                this.httpNodeServices
            );
        this.containersMap[coord.layer][coord.index] = container;
        return container;
    }
}