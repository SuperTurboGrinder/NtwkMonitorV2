import { Component, Output, EventEmitter, Input } from '@angular/core';

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
            this.sortParamPositions();
        }
        this.buildFinalTemplate();
    }

    private sortParamPositions() {
        this.paramPositions.sort((p1, p2) => {
            const firstParam = this.params[p1];
            const secondParam = this.params[p2];
            return firstParam.strPos < secondParam.strPos
                ? -1
                : secondParam.strPos < firstParam.strPos
                    ? 1 : 0;
        });
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
    }

    isLeftEdge(paramNum: number) {
        const param = this.params[paramNum];
        return param.strPos === 0
            && (paramNum !== 0 || this.paramPositions[0] === paramNum);
    }

    isRightEdge(paramNum: number) {
        const param = this.params[paramNum];
        return param.strPos === this.baseUrl.length
            && (paramNum !== 0 ||
                this.paramPositions[this.paramPositions.length - 1] === paramNum
            );
    }

    private move(paramNum: number, negative: boolean = false) {
        const moveDirection = negative ? -1 : 1;
        const param = this.params[paramNum];
        let skipStrPosMove = false;
        const isInEdgePositon =
            this.paramPositions[
                negative ? 0 : this.paramPositions.length - 1
            ] === paramNum;
        if (!isInEdgePositon) {
            const index = this.paramPositions.findIndex(p => p === paramNum);
            const nextParam =
                this.params[this.paramPositions[index + moveDirection]];
            if (nextParam.strPos === param.strPos) {
                if (param.paramNum === 0 || nextParam.paramNum === 0) {
                    this.paramPositions[index] = nextParam.paramNum;
                    this.paramPositions[index + moveDirection] = paramNum;
                    skipStrPosMove = true;
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
