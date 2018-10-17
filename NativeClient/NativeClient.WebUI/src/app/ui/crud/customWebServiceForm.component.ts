import { Component } from "@angular/core";
import { Location } from "@angular/common";
import { ActivatedRouteSnapshot, ActivatedRoute } from "@angular/router";
import { HTTPResult } from "../../model/servicesModel/httpResult.model";
import { Range } from "../misc/numRangeSelector.component"
import { MessagesEnum } from "src/app/model/servicesModel/messagesEnum.model";
import { MessagingService } from "src/app/services/messaging.service";
import { BaseCrudFormComponent } from "../helpers/baseCrudFormComponent.helper";
import { CustomWebServicesService } from "src/app/services/customWebServices.service";
import { CustomWebService } from "src/app/model/httpModel/customWebService.model";

@Component({
    selector: 'customWebServiceForm',
    templateUrl: './customWebServiceForm.component.html'
})
export class CustomWebServiceFormComponent
    extends BaseCrudFormComponent<CustomWebService, CustomWebServicesService> {
    
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
                let cws = cwsResult.success === true
                    ? cwsResult.data.find(cws => cws.id === id)
                    : null;
                callback(
                    cwsResult.success,
                    cws
                );
            }
        )
    }

    protected newEmptyData(): CustomWebService {
        return {
            id: 0,
            name: "",
            serviceStr: "",
            parametr1Name: null,
            parametr2Name: null,
            parametr3Name: null
        };
    }

    protected currentIdenticalTo(obj: CustomWebService): boolean {
        return obj.name === this.data.name
            && obj.serviceStr === this.data.serviceStr
            && obj.parametr1Name === this.data.parametr1Name
            && obj.parametr2Name === this.data.parametr2Name
            && obj.parametr3Name === this.data.parametr3Name;
    }

    protected makeCopy(orig: CustomWebService) : CustomWebService {
        return {
            id: orig.id,
            name: orig.name,
            serviceStr: orig.serviceStr,
            parametr1Name: orig.parametr1Name,
            parametr2Name: orig.parametr2Name,
            parametr3Name: orig.parametr3Name
        };
    }

    protected saveAsNewObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.createNewCWS(
            this.data,
            callback
        );
    }

    protected saveChangesToObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.updateCWS(
            this.data,
            callback
        );
    }
}