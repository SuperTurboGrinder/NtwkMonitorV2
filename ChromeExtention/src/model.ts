export class CWSData {
    constructor(
        public id: number,
        public name: string
    ) {}
}

export class NtwkNode {
    constructor(
        public id: number,
        public parentID: number,
        public parentPort: number,
        public name: string,
        public ipStr: string,
        public isOpenSSH: boolean,
        public isOpenTelnet: boolean,
        public isOpenPing: boolean
    ) {}
}

export class NodeData {
    constructor(
        public node: NtwkNode,
        public tagsIDs: number[],
        public boundWebServicesIDs: number[]
    ) {}
}

export class AllNodesData {
    constructor(
        public webServicesData: CWSData[],
        public nodesData: NodeData[][]
    ) {}
}

export class NodeTag {
    constructor(
        public id: number,
        public name: string
    ) {}
}
