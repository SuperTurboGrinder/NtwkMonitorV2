export class CWSBondExistanceMapping {
    constructor(
        public cwsIDs:number[],
        public bindings:CWSBondExistanceData[]
    ) {}
}

export class CWSBondExistanceData {
    constructor(
        public nodeID:number,
        public bind:boolean[]
     ) {}
}