import { Component, Input } from '@angular/core';
import { WebServicesService } from '../../services/webServices.service';

@Component({
    selector: 'app-web-services-buttons',
    templateUrl: './webServicesButtons.component.html'
})
export class WebServicesButtonsComponent {
    _webServices: {
        name: string,
        id: number
    }[] = [];

    @Input() nodeID: number;
    @Input() set webServices(ws: {
        name: string,
        id: number
    }[]) {
        this._webServices = ws;
    }

    serviceTrackBy(index: number, ws: {
        name: string,
        id: number
    }) {
        return ws.id;
    }

    get webServices(): {
        name: string,
        id: number
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
