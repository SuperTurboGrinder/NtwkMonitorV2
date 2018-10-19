import { Component, Input } from '@angular/core';
import { WebServicesService } from '../../services/webServices.service';

@Component({
    selector: 'app-web-services-buttons',
    templateUrl: './webServicesButtons.component.html'
})
export class WebServicesButtonsComponent {
    _webServices: {
        name: string,
        serviceID: number
    }[] = [];

    @Input() nodeID: number;
    @Input() set webServices(ws: {
        name: string,
        serviceID: number
    }[]) {
        this._webServices = ws;
    }

    serviceTrackBy(index: number, ws: {
        name: string,
        serviceID: number
    }) {
        return ws.serviceID;
    }

    get webServices(): {
        name: string,
        serviceID: number
    }[] {
        return this._webServices;
    }

    constructor(
        private webServicesService: WebServicesService
    ) {}

    public runCustomWebService(serviceID: number) {
        this.webServicesService.runCustomWebService(this.nodeID, serviceID);
    }
}
