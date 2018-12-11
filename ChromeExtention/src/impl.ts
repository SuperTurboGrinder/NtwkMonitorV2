export class Implementation {
    private readonly _nodeNames: string[];

    constructor() {
        // set nodeNames to a list of selected nodes
        this._nodeNames = [];
    }

    get nodeNames(): string[] {
        return this._nodeNames;
    }
}