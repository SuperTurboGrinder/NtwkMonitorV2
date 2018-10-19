import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CustomWebService } from 'src/app/model/httpModel/customWebService.model';

class Param {
    paramNum: number; // 0-ip, 1-3 params
    strPos: number;
}

@Component({
    selector: 'app-custom-web-service-string-builder',
    templateUrl: './customWebServiceStringBuilder.component.html'
})
export class CustomWebServiceStringBuilderComponent {
    readonly from1To3 = [1, 2, 3];
    private paramNames = ['', '', ''];
    private paramNum = 0; // 0-3
    private ipUsage = false;
    private baseUrl = '';
    private finalUrlTemplate = '';
    private params: Param[] = [
        { paramNum: 0, strPos: 0 }, // ip
        { paramNum: 1, strPos: 0 },
        { paramNum: 2, strPos: 0 },
        { paramNum: 3, strPos: 0 }
    ];
    private paramPositions: number[] = [
        1, 2, 3 // 0-ip, 1-3 params
    ];

    public useHttp = false;

    @Output() private changeEvent = new EventEmitter<{
        templateStr: string,
        param1Name: string,
        param2Name: string,
        param3Name: string
    }>();

    private initialized = false;
    @Input() private set initialValue(val: CustomWebService) {
        if (val !== null && !this.initialized) {
            const httpStr = 'http://';
            const httpsStr = 'https://';
            const nodeIPStr = '{node_ip}';
            const param1Str = '{param1}';
            const param2Str = '{param2}';
            const param3Str = '{param3}';
            let rawTemplate: string = null;
            if (val.serviceStr.includes(httpStr)) {
                this.useHttp = true;
                rawTemplate = val.serviceStr.replace(httpStr, '');
            } else {
                this.useHttp = false;
                rawTemplate = val.serviceStr.replace(httpsStr, '');
            }
            const finalUrlTemplate = rawTemplate;
            this.setIPUsage(false);
            if (val.serviceStr.includes(nodeIPStr)) {
                const templateWithIP = rawTemplate
                    .replace(param1Str, '')
                    .replace(param2Str, '')
                    .replace(param3Str, '');
                this.setIPUsage(true);
                this.params[0].strPos = templateWithIP.indexOf(nodeIPStr);
                rawTemplate.replace(nodeIPStr, '');
            }
            const hasParam1 = val.parametr1Name !== null && val.parametr1Name !== '';
            const hasParam2 = val.parametr2Name !== null && val.parametr2Name !== '';
            const hasParam3 = val.parametr3Name !== null && val.parametr3Name !== '';
            this.paramNum = 0;
            if (hasParam1) {
                this.paramNames[0] = val.parametr1Name;
                this.paramNum = 1;
                this.params[1].strPos = rawTemplate.indexOf(param1Str);
                rawTemplate = rawTemplate.replace(param1Str, '');
                if (hasParam2) {
                    this.paramNames[1] = val.parametr2Name;
                    this.paramNum = 2;
                    this.params[2].strPos = rawTemplate.indexOf(param2Str);
                    rawTemplate = rawTemplate.replace(param2Str, '');
                    if (hasParam3) {
                        this.paramNames[2] = val.parametr3Name;
                        this.paramNum = 3;
                        this.params[3].strPos = rawTemplate.indexOf(param3Str);
                        rawTemplate = rawTemplate.replace(param3Str, '');
                    }
                }
            }
            this.finalUrlTemplate = finalUrlTemplate;
            this.baseUrl = rawTemplate;
            this.emitChange();
        }
    }

    private emitChange() {
        this.changeEvent.emit({
            templateStr: this.useHttp ? 'http://' : 'https://' + this.finalUrlTemplate,
            param1Name: this.paramNames[0] === '' ? null : this.paramNames[0],
            param2Name: this.paramNames[1] === '' ? null : this.paramNames[1],
            param3Name: this.paramNames[2] === '' ? null : this.paramNames[2]
        });
    }

    getFinalUrlTemplate(): string {
        return this.finalUrlTemplate;
    }

    getBaseUrl() {
        return this.baseUrl;
    }

    setBaseUrl(newValue: string) {
        this.baseUrl = newValue;
        this.fitParamsStrPos();
        this.buildFinalTemplate();
    }

    getParamName(paramNum: number) {
        return this.paramNames[paramNum - 1];
    }

    setParamName(paramNum: number, newName: string) {
        this.paramNames[paramNum - 1] = newName;
    }

    private fitParamsStrPos() {
        const maxPos = this.baseUrl.length;
        for (const param of this.params) {
            param.strPos = param.strPos > maxPos ? maxPos : param.strPos;
        }
    }

    usesParam(paramNum: number): boolean {
        return this.paramNum >= paramNum;
    }

    setParamUsage(on: boolean, paramNum: number) {
        if (on) {
            this.paramNum = paramNum;
        } else {
            this.paramNum = paramNum - 1;
        }
        if (this.params[2].strPos < this.params[1].strPos) {
            this.params[2].strPos = this.params[1].strPos;
        }
        if (this.params[3].strPos < this.params[2].strPos) {
            this.params[3].strPos = this.params[2].strPos;
        }
        this.buildFinalTemplate();
    }

    useIP(): boolean {
        return this.ipUsage;
    }

    setIPUsage(on: boolean) {
        this.ipUsage = on;
        if (on === false) {
            this.paramPositions = this.paramPositions.filter(p => p !== 0);
        } else {
            this.paramPositions.push(0);
            this.params[0].strPos = this.baseUrl.length;
        }
        this.buildFinalTemplate();
    }

    buildFinalTemplate() {
        let currentCursor = 0;
        const stringBuilder: string[] = [];
        for (const paramIndex of this.paramPositions) {
            if (paramIndex !== 0 && paramIndex > this.paramNum) {
                continue;
            }
            const param = this.params[paramIndex];
            const prefix = this.baseUrl.substr(currentCursor, param.strPos - currentCursor);
            currentCursor = param.strPos;
            const value = param.paramNum === 0
                ? '{node_ip}' : `{param${param.paramNum}}`;
            stringBuilder.push(prefix, value);
        }
        const suffix = this.baseUrl.substr(currentCursor, this.baseUrl.length - currentCursor);
        stringBuilder.push(suffix);
        this.finalUrlTemplate = stringBuilder.join('');
        this.emitChange();
    }

    isLeftEdge(paramNum: number): boolean {
        const param = this.params[paramNum];
        return param.strPos === 0
            && (paramNum !== 0 || this.paramPositions[0] === paramNum);
    }

    isRightEdge(paramNum: number): boolean {
        const param = this.params[paramNum];
        return param.strPos === this.baseUrl.length
            && (paramNum !== 0 ||
                this.paramPositions[this.paramPositions.length - 1] === paramNum
            );
    }

    private sameStrPosWithNext(negative: boolean, paramNum: number): boolean {
        const nextParamNum = this.paramPositions[
            this.paramPositions.indexOf(paramNum + (negative ? -1 : 1))
        ];
        return this.params[paramNum].strPos === this.params[nextParamNum].strPos;
    }

    private isOnEdge(negative: boolean, paramNum: number) {
        return this.paramPositions[
            negative ? 0 : this.paramPositions.length - 1
        ] === paramNum;
    }

    private shouldSwapWithNext(negative: boolean, paramNum: number): boolean {
        if (paramNum === 0) { // node_ip
            if (this.paramNum === 0) { // swap through all
                return true;
            }
            if (this.paramNum === 3) {
                return this.sameStrPosWithNext(negative, paramNum);
            }
            if (negative) {
                return this.paramPositions.indexOf(paramNum) > this.paramNum
                    || this.sameStrPosWithNext(negative, paramNum);
            } else {
                return this.paramPositions.indexOf(paramNum) < this.paramNum - 1
                    || this.sameStrPosWithNext(negative, paramNum);
            }
        } else { // paramN
            const nextIndex = this.paramPositions.indexOf(paramNum)
                + (negative ? -1 : 1);
            const nextIsIP = this.paramPositions[nextIndex] === 0;
            return nextIsIP && this.sameStrPosWithNext(negative, paramNum);
        }
    }

    private swapWithNext(negative: boolean, paramNum: number) {
        const index = this.paramPositions.indexOf(paramNum);
        const nextIndex = this.paramPositions
            .indexOf(paramNum + (negative ? -1 : 1));
        const nextParamNum = this.paramPositions[nextIndex];
        this.paramPositions[index] = nextParamNum;
        this.paramPositions[nextIndex] = paramNum;
    }

    private move(paramNum: number, negative: boolean = false) {
        if (!this.isOnEdge(negative, paramNum)) {
            if (this.shouldSwapWithNext(negative, paramNum)) {
                this.swapWithNext(negative, paramNum);
            }
        }
        const moveDirection = negative ? -1 : 1;
        const param = this.params[paramNum];
        let skipStrPosMove = false;
        const isInEdgePositon = paramNum === 0
            ? this.paramNum === 0
                ? true
                : negative
                    ? this.paramPositions.indexOf(0) === 0
                    : this.paramPositions.indexOf(0) >= this.paramNum
            : this.paramPositions[
                negative ? 0 : this.paramPositions.length - 1
            ] === paramNum;
        if (!isInEdgePositon) {
            let index = this.paramPositions.indexOf(paramNum);
            let nextIndex = index + moveDirection;
            let nextParam =
                this.params[this.paramPositions[nextIndex]];
            if (paramNum === 0 && negative) {
                while (nextParam.paramNum > this.paramNum) {
                    this.paramPositions[index] = nextParam.paramNum;
                    this.paramPositions[nextIndex] = paramNum;
                    index = nextIndex;
                    nextIndex += moveDirection;
                    nextParam =
                        this.params[this.paramPositions[nextIndex]];
                }
            }
            console.log(this.paramPositions);
            console.log(param);
            console.log(nextParam);
            if (nextParam.strPos === param.strPos) {
                if (param.paramNum === 0 || nextParam.paramNum === 0) {
                    this.paramPositions[index] = nextParam.paramNum;
                    this.paramPositions[nextIndex] = paramNum;
                    skipStrPosMove = true;
                    console.log(this.paramPositions);
                } else {
                    this.move(nextParam.paramNum, negative);
                }
            }
        }
        if (!skipStrPosMove) {
            const isOnStringEdge = !negative
                ? param.strPos === this.baseUrl.length
                : param.strPos === 0;
            if (!isOnStringEdge) {
                param.strPos += moveDirection;
            }
        }
        this.buildFinalTemplate();
    }

    moveForward(paramNum: number) {
        this.move(paramNum);
    }

    moveBackward(paramNum: number) {
        this.move(paramNum, true);
    }
}
