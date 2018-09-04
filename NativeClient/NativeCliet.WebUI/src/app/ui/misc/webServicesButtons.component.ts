import { Component, EventEmitter, Input } from "@angular/core";
import { WebServicesService } from "../../services/webServices.service";

@Component({
    selector: 'webServicesButtons',
    templateUrl: './webServicesButtons.component.html'
})
export class WebServicesButtonsComponent {
    @Input() nodeID: number;
    @Input() webServices: {
        name:string,
        serviceID:number
    }[];

    constructor(
        private webServicesService: WebServicesService
    ) {}

    public runCustomWebService(serviceID:number) {
        this.webServicesService.runCustomWebService(this.nodeID, serviceID);
    }
}