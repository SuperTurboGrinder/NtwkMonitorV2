import { SmartNodeContainer } from "./nodesPacking";
import { PingTestData } from "./model";
import { Implementation } from "./impl.interface";

export class TestImpl implements Implementation {
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
        this.testPageParser.addServices(requestedNodes);
    }
}

// test and demonstation class
class TestPageParsing {
    private containers: HTMLElement[] = null;
    private names: string[] = null;

    constructor() {
        this.initPageData();
    }

    getNames(): string[] {
        return this.names;
    }

    addServices(nodeContainers: SmartNodeContainer[]) {
        nodeContainers.map((node, index) => {
            if (node === null) {
                return;
            }
            if (node.hasTelnet) {
                this.addButton(index, 'telnet', () => node.openTelnet());
            }
            if (node.hasSSH) {
                this.addButton(index, 'ssh', () => node.openSSH());
            }
            this.addWsButtons(index, node.webServicesNames, node);
            if (node.hasPing) {
                this.addPingBlock(
                    index,
                    (callback: (pd: PingTestData) => void) => {
                        node.getPing(callback);
                    }
                );
            }
            this.addWsHrefs(index, node.webServicesNames, node);
        });
    }

    private addWsButtons(elementIndex: number, wsNames: string[], node: SmartNodeContainer) {
        const block = document.createElement('span');
        block.style.padding = '2px';
        for (let i = 0; i < wsNames.length; i++) {
            this.addButtonToElement(
                block,
                wsNames[i],
                () => node.openWebService(wsNames[i])
            );
        }
        this.elementByIndex(elementIndex).appendChild(block);
    }

    private addWsHrefs(elementIndex: number, wsNames: string[], node: SmartNodeContainer) {
        const block = document.createElement('div');
        block.style.padding = '2px';
        for (let i = 0; i < wsNames.length; i++) {
            node.getWebServiceString(wsNames[i], href => {
                const a = document.createElement('a');
                a.innerHTML = href;
                a.href = href;
                block.appendChild(a);
                block.appendChild(document.createElement('br'));
            });
        }
        this.elementByIndex(elementIndex).appendChild(block);
    }

    private addButton(elementIndex: number, name: string,  func: () => void) {
        const elem = this.elementByIndex(elementIndex);
        this.addButtonToElement(elem, name, func);
    }

    private addPingBlock(
        elementIndex: number,
        func: (callback: (pingData: PingTestData) => void) => void
    ) {
        const elem = this.elementByIndex(elementIndex);
        const block = document.createElement('span');
        block.style.padding = '2px';
        const valueLabel = document.createElement('span');
        valueLabel.innerText = this.makePingOutput(null);
        this.addButtonToElement(block, 'Ping', () => {
            valueLabel.innerText = this.makePingOutput(null);
            func((pd: PingTestData) => {
                valueLabel.innerText = this.makePingOutput(pd);
            });
        });
        block.appendChild(valueLabel);
        elem.appendChild(block);
    }

    private makePingOutput(ptd: PingTestData): string {
        if (ptd === null) {
            return '- -/-';
        }
        return `${ptd.avg} ${ptd.num - ptd.failed}/${ptd.num}`;
    }
 
    private elementByIndex(index: number) {
        return this.containers[index];
    }

    private addButtonToElement(elem: HTMLElement, name: string,  func: () => void) {
        const button = document.createElement('button');
        button.type = 'button';
        button.innerHTML = name;
        button.style.margin = '2px';
        button.addEventListener('click', func);
        elem.appendChild(button);
    }

    private initPageData() {
        this.containers = Array.from(document.getElementsByTagName('div'));
        this.names = this.containers.map(e => e.innerText); 
    }
}