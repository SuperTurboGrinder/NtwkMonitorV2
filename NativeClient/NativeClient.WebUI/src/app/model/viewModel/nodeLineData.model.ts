export class NodeLineData {
    constructor(
        public id: number,
        public name: string,
        public prefix: string,
        public depth: number,
        public hasChildren: boolean,
        public infoIndex: number
    ) { }
}
