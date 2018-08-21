export class NtwkNode {
    constructor(
        public id:number,
        public parentID:number,
        public parentPort:number,
        public name:string,
        public ipStr:string,
        public isOpenSSH:boolean,
        public isOpenTelnet:boolean,
        public isOpenPing:boolean
    ) {}
}