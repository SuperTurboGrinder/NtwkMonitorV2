import { Component, Output, EventEmitter, Input } from "@angular/core";

@Component({
    selector: 'customWebServiceStringBuilder',
    templateUrl: './customWebServiceStringBuilder.component.html'
})
export class CustomWebServiceStringBuilderComponent {
    private paramNum: number = 0; //0-3
    private ipUsage: boolean;
    private ipPosition: number;
    private paramPositions = [0, 1, 2];
    private baseUrl: string = "";
    private finalUrlTemplate = "";
    private urlTemplateFragments: [{
        isParam: boolean,
        paramNum: number,
        value: string
    }]

    public useHttp = false;

    usesParam1(): boolean {
        return this.paramNum >= 1;
    }

    usesParam2(): boolean {
        return this.paramNum >= 2;
    }

    usesParam3(): boolean {
        return this.paramNum >= 3;
    }

    setParm1Usage(on: boolean) {
        if(on)
            this.paramNum = 1;
        else
            this.paramNum = 0;
    }

    setParm2Usage(on: boolean) {
        if(on)
            this.paramNum = 2;
        else
            this.paramNum = 1;
    }

    setParm3Usage(on: boolean) {
        if(on)
            this.paramNum = 3;
        else
            this.paramNum = 2;
    }

    useIP(): boolean {
        return this.ipUsage;
    }

    setIPUsage(on: boolean) {
        this.ipUsage = on;
    }

    buildUrlFragments() {
        let params = this.paramPositions.slice(0, this.paramNum)
            .map((pos, index) => {
                return { num: index+1, pos: pos }
            })
            .sort((v1, v2) => v1.pos < v2.pos ? -1 : v2.pos < v1.pos ? 1 : 0);
        if(params.length > 0) {

        } else {
            this.urlTemplateFragments = [ {
                isParam: false,
                paramNum: 0,
                value: this.baseUrl
            } ]
        }
    }
}