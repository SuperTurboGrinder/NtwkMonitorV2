import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { BaseCrudFormComponent } from '../helpers/baseCrudFormComponent.helper';
import { CustomWebServicesService } from 'src/app/services/customWebServices.service';
import { CustomWebService } from 'src/app/model/httpModel/customWebService.model';

class Param {
    paramNum: number; // 0-ip, 1-3 params
    strPos: number;
}

@Component({
    selector: 'app-custom-web-service-form',
    templateUrl: './customWebServiceForm.component.html'
})
export class CustomWebServiceFormComponent
    extends BaseCrudFormComponent<CustomWebService, CustomWebServicesService> {

    readonly from1To3 = [1, 2, 3];
    private paramNum = 0; // 0-3
    private ipUsage = false;
    private baseUrl = '';
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

    constructor(
        messager: MessagingService,
        location: Location,
        route: ActivatedRoute,
        cwsService: CustomWebServicesService
    ) {
        super(messager, location, route, cwsService);
    }

    protected getOriginalData(
        id: number,
        callback: (success: boolean, orig: CustomWebService) => void
    ) {
        this.dataService.getCWSList().subscribe(
            (cwsResult: HTTPResult<CustomWebService[]>) => {
                const cws = cwsResult.success === true
                    ? cwsResult.data.find(s => s.id === id)
                    : null;
                callback(
                    cwsResult.success,
                    cws
                );
                this.initFormData(cws);
            }
        );
    }

    protected newEmptyData(): CustomWebService {
        return {
            id: 0,
            name: '',
            serviceStr: '',
            parametr1Name: '',
            parametr2Name: '',
            parametr3Name: ''
        };
    }

    protected currentIdenticalTo(obj: CustomWebService): boolean {
        return obj.name === this.data.name
            && obj.serviceStr === this.data.serviceStr
            && obj.parametr1Name === this.data.parametr1Name
            && obj.parametr2Name === this.data.parametr2Name
            && obj.parametr3Name === this.data.parametr3Name;
    }

    protected makeCopy(orig: CustomWebService): CustomWebService {
        return {
            id: orig.id,
            name: orig.name,
            serviceStr: orig.serviceStr,
            parametr1Name: orig.parametr1Name,
            parametr2Name: orig.parametr2Name,
            parametr3Name: orig.parametr3Name
        };
    }

    private prepareSendingData(): CustomWebService {
        const sendingData = this.makeCopy(this.data);
        sendingData.parametr1Name = this.paramNum < 1
            ? null : sendingData.parametr1Name;
        sendingData.parametr2Name = this.paramNum < 2
            ? null : sendingData.parametr2Name;
        sendingData.parametr3Name = this.paramNum < 3
            ? null : sendingData.parametr3Name;
        return sendingData;
    }

    protected saveAsNewObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        const sendingData = this.prepareSendingData();
        this.dataService.createNewCWS(
            sendingData,
            callback
        );
    }

    protected saveChangesToObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        const sendingData = this.prepareSendingData();
        this.dataService.updateCWS(
            sendingData,
            callback
        );
    }


    // actual form logic
    private initFormData(val: CustomWebService) {
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
            rawTemplate = rawTemplate.replace(nodeIPStr, '');
        }
        const hasParam1 = val.parametr1Name !== null && val.parametr1Name !== '';
        const hasParam2 = val.parametr2Name !== null && val.parametr2Name !== '';
        const hasParam3 = val.parametr3Name !== null && val.parametr3Name !== '';
        this.paramNum = 0;
        if (hasParam1) {
            this.paramNum = 1;
            this.params[1].strPos = rawTemplate.indexOf(param1Str);
            rawTemplate = rawTemplate.replace(param1Str, '');
            if (hasParam2) {
                this.paramNum = 2;
                this.params[2].strPos = rawTemplate.indexOf(param2Str);
                rawTemplate = rawTemplate.replace(param2Str, '');
                if (hasParam3) {
                    this.paramNum = 3;
                    this.params[3].strPos = rawTemplate.indexOf(param3Str);
                    rawTemplate = rawTemplate.replace(param3Str, '');
                }
            }
        }
        if (this.ipUsage) {
            let ipPos = 3;
            while (ipPos !== 0
                && this.params[this.paramPositions[--ipPos]].strPos > this.params[0].strPos
            ) {
                this.swapParamsByIndex(ipPos + 1, ipPos);
            }
        }
        this.data.serviceStr = (this.useHttp ? httpStr : httpsStr) + finalUrlTemplate;
        this.baseUrl = rawTemplate;
    }

    public swapUseHttp() {
        this.useHttp = !this.useHttp;
        this.buildFinalTemplate();
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
        switch (paramNum) {
            case 1: return this.data.parametr1Name;
            case 2: return this.data.parametr2Name;
            case 3: return this.data.parametr3Name;
        }
    }

    setParamName(paramNum: number, newName: string) {
        switch (paramNum) {
            case 1: this.data.parametr1Name = newName;
            break;
            case 2: this.data.parametr2Name = newName;
            break;
            case 3: this.data.parametr3Name = newName;
            break;
        }
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
        this.data.serviceStr = (this.useHttp ? 'http://' : 'https://') + stringBuilder.join('');
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

    private isSameStrPosByIndex(paramPos1Index: number, paramPos2Index: number): boolean {
        const paramNum = this.paramPositions[paramPos1Index];
        const nextParamNum = this.paramPositions[paramPos2Index];
        return this.params[paramNum].strPos === this.params[nextParamNum].strPos;
    }

    private isOnEdge(negative: boolean, paramNum: number) {
        return this.paramPositions[
            negative ? 0 : this.paramPositions.length - 1
        ] === paramNum;
    }

    private getParamPosIndex(paramNum: number): number {
        return this.paramPositions.indexOf(paramNum);
    }

    private nextParamIndex(negative: boolean, paramPosIndex: number) {
        return paramPosIndex + (negative ? -1 : 1);
    }

    private canSwapWithNext(negative: boolean, paramPosIndex: number): boolean {
        const paramNum = this.paramPositions[paramPosIndex];
        if (paramNum === 0) { // node_ip
            if (this.paramNum === 0) { // swap through all
                return true;
            }
            const nextPosIndex = this.nextParamIndex(negative, paramPosIndex);
            if (this.paramNum === 3) {
                return this.isSameStrPosByIndex(paramPosIndex, nextPosIndex);
            }
            if (negative) {
                return paramPosIndex > this.paramNum
                    || this.isSameStrPosByIndex(paramPosIndex, nextPosIndex);
            }
            return paramPosIndex < this.paramNum - 1
                || this.isSameStrPosByIndex(paramPosIndex, nextPosIndex);
        } else { // paramN
            const nextPosIndex = this.nextParamIndex(negative, paramPosIndex);
            const nextIsIP = this.paramPositions[nextPosIndex] === 0;
            return nextIsIP && this.isSameStrPosByIndex(paramPosIndex, nextPosIndex);
        }
    }

    private swapParamsByIndex(paramPos1Index: number, paramPos2Index: number) {
        const paramNum = this.paramPositions[paramPos1Index];
        this.paramPositions[paramPos1Index] = this.paramPositions[paramPos2Index];
        this.paramPositions[paramPos2Index] = paramNum;
    }

    private move(paramNum: number, negative: boolean = false) {
        // console.log('Before:');
        // console.log(this.paramPositions);
        // console.log(
        //     `p1s: ${this.params[1].strPos}, p2s: ${this.params[2].strPos}, p3s: ${this.params[3].strPos}, ips: ${this.params[0].strPos}`
        // );
        let isParamOnEdge = this.isOnEdge(negative, paramNum);
        const parametr = this.params[paramNum];
        const nextStrPos = parametr.strPos + (negative ? -1 : 1);
        const canMoveStrPos = nextStrPos >= 0 && nextStrPos <= this.baseUrl.length;
        if (!isParamOnEdge) {
            let paramPosIndex = this.getParamPosIndex(paramNum);
            let shouldSwap = this.canSwapWithNext(negative, paramPosIndex);
            let nextParamPosIndex = this.nextParamIndex(negative, paramPosIndex);
            if (shouldSwap) {
                while (!isParamOnEdge && shouldSwap) {
                    this.swapParamsByIndex(paramPosIndex, nextParamPosIndex);
                    if (this.paramNum !== 0) {
                        break;
                    }
                    isParamOnEdge = this.isOnEdge(negative, paramNum);
                    if (!isParamOnEdge) {
                        shouldSwap = this.canSwapWithNext(negative, nextParamPosIndex);
                        paramPosIndex = nextParamPosIndex;
                        nextParamPosIndex = this.nextParamIndex(negative, paramPosIndex);
                    }
                }
            } else {
                if (canMoveStrPos) {
                    let nextParam = this.params[this.paramPositions[nextParamPosIndex]];
                    let shouldPushNext = parametr.strPos ===
                        this.params[this.paramPositions[nextParamPosIndex]].strPos;
                    let isNextParamOnEdge = false;
                    while (!isNextParamOnEdge && shouldPushNext) {
                        nextParam.strPos = nextStrPos;
                        isNextParamOnEdge = this.isOnEdge(negative, nextParam.paramNum);
                        if (!isNextParamOnEdge) {
                            nextParamPosIndex = this.nextParamIndex(negative, nextParamPosIndex);
                            nextParam = this.params[this.paramPositions[nextParamPosIndex]];
                            shouldPushNext = nextParam.strPos === parametr.strPos;
                        }
                    }
                    parametr.strPos = nextStrPos;
                }
            }
        } else if (canMoveStrPos) {
            parametr.strPos = nextStrPos;
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
