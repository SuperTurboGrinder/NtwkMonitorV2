export class NtwkNode {
    constructor(
        public id: number,
        public parentId: number,
        public parentPort: number,
        public name: string,
        public ipStr: string,
        public isOpenSsh: boolean,
        public isOpenTelnet: boolean,
        public isOpenPing: boolean
    ) {}
}
