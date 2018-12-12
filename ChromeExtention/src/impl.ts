import { SmartNodeContainer } from "./nodesPacking";

export class Implementation {
    private readonly _nodesNames: string[];

    // demonstration page parser
    private readonly testPageParser = new TestPageParsing();

    constructor() {
        // set nodeNames to a list of selected nodes
        // even if it is single one
        this._nodesNames = this.testPageParser.getNames();
    }

    get nodesNames(): string[] {
        return this._nodesNames;
    }

    useNodes(requestedNodes: SmartNodeContainer[]) {
        
    }
}

// test and demonstation class
class TestPageParsing {
    private containers: HTMLElement[] = null;
    private names: string[] = null;

    constructor() {
        this.initPageData();
    }

    getNames() {
        return this.names;
    }

    addServices(nodeContainers: SmartNodeContainer[]) {
        nodeContainers.map((node, index) => {
            if (node.hasTelnet) {
                this.addButton(index, 'telnet', () => node.openTelnet());
            }
            if (node.hasSSH) {
                this.addButton(index, 'ssh', () => node.openSSH());
            }
            if (node.hasPing) {
                
            }
        });
    }

    private addButton(elementIndex: number, name: string,  func: () => void) {
        const element = this.containers[elementIndex];
        const button = document.createElement('button');
        button.type = 'button';
        button.name = name;
        button.click = func;
    }

    private initPageData() {
        this.containers = Array.from(document.getElementsByName('div'));
        this.names = this.containers.map(e => e.innerText);
    }
}