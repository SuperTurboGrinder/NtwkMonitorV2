import { Component, EventEmitter, Input } from "@angular/core";
import { WebServicesService } from "../../services/webServices.service";

@Component({
    selector: 'webServicesButtons',
    templateUrl: './webServicesButtons.component.html'
})
export class WebServicesButtonsComponent {
    _webServices: {
        name:string,
        serviceID:number
    }[] = [];
    
    @Input() nodeID: number;
    @Input() set webServices(ws:{
        name:string,
        serviceID:number
    }[]) {
        this._webServices = ws;
        //this._webServices = [];
        //for(let i = 0; i<20; i++) {
        //    this._webServices.push({name:i.toString(), serviceID:7});
        //}
    }

    serviceTrackBy(index: number, ws: {
        name:string,
        serviceID:number
    }) {
        return ws.serviceID;
    }

    get webServices() : {
        name:string,
        serviceID:number
    }[] {
        return this._webServices;
    }

    constructor(
        private webServicesService: WebServicesService
    ) {}

    public runCustomWebService(serviceID:number) {
        this.webServicesService.runCustomWebService(this.nodeID, serviceID);
    }
}