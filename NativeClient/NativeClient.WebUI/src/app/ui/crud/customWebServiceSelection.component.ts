import { Component } from "@angular/core";
import { CustomWebService } from "../../model/httpModel/customWebService.model";
import { MessagingService } from "src/app/services/messaging.service";
import { BaseCrudSelectorComponent } from "../helpers/baseCrudSelectorComponent.helper";
import { CustomWebServicesService } from "src/app/services/customWebServices.service";

@Component({
    selector: 'customWebServiceSelection',
    templateUrl: './customWebServiceSelection.component.html'
})
export class CustomWebServiceSelectionComponent
    extends BaseCrudSelectorComponent<CustomWebService, CustomWebServicesService> {
    constructor(
        messager: MessagingService,
        dataService: CustomWebServicesService
    ) {
        super(messager, dataService);
    }

    protected updateDataList() {
        this.dataService.getCWSList().subscribe(
            cwsListResult => this.setNewData(cwsListResult)
        );
    }

    protected deleteObjectPermanently(
        objID: number,
        callback: (success: boolean) => void
    ) {
        this.dataService.deleteCWS(objID, callback);
    }
}