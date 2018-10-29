import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';
import { CustomWebServicesService } from 'src/app/services/customWebServices.service';
import { CustomWebService } from 'src/app/model/httpModel/customWebService.model';
import { CustomWebServiceBinding } from 'src/app/model/httpModel/cwsBindingData.model';

@Component({
    selector: 'app-custom-web-service-binding',
    templateUrl: './customWebServiceBinding.component.html'
})
export class CustomWebServiceBindingComponent {
    private services: CustomWebService[] = null;
    availableServices: CustomWebService[] = [];
    setServices: CustomWebService[] = [];
    selectedService: CustomWebService = null;
    formWebServiceBinding: CustomWebServiceBinding = new CustomWebServiceBinding(1, '', '', '');
    private _originalBountServices: number[] = null;
    private selectedNodeID = 0;

    @Input() set allServices(services: CustomWebService[]) {
        this.services = services;
        this.updateBoundServices();
    }

    @Output() private bindingEvent = new EventEmitter<{
        serviceID: number
        bindingData: CustomWebServiceBinding
    }>();

    @Output() private bindingRemovalEvent = new EventEmitter<number>();

    @Input() set originalBoundServicesData(data: {
        nodeID: number
        servicesIDs: number[]
    }) {
        if (data !== null && this._originalBountServices !== data.servicesIDs) {
            this.selectedNodeID = data.nodeID;
            this._originalBountServices = data.servicesIDs.sort((a, b) => a - b);
            if (this.services !== null) {
                this.updateBoundServices();
                if (this.selectedService !== null) {
                    const selectedAvailable = this.availableServices.find(
                        s => s.id === this.selectedService.id
                    );
                    this.selectedService = selectedAvailable === undefined
                        ? null
                        : selectedAvailable;
                }
            }
        }
    }

    public get isNoSelection(): boolean {
        return this._originalBountServices === null;
    }

    public selectService(cws: CustomWebService) {
        this.selectedService = cws;
    }

    public removeServiceBinding(cws: CustomWebService) {
        this.bindingRemovalEvent.emit(cws.id);
    }

    public bindSelectedService() {
        const sendingServiceBinding = new CustomWebServiceBinding(
            this.selectedNodeID,
            this.selectedService.parametr1Name === null
                ? null : this.formWebServiceBinding.param1,
            this.selectedService.parametr2Name === null
                ? null : this.formWebServiceBinding.param2,
            this.selectedService.parametr3Name === null
                ? null : this.formWebServiceBinding.param3,
        );
        this.bindingEvent.emit({
            serviceID: this.selectedService.id,
            bindingData: sendingServiceBinding
        });
        this.selectedService = null;
    }

    private updateBoundServices() {
        if (this._originalBountServices !== null) {
            this.setServices = this.services
                .filter(s => this._originalBountServices.includes(s.id))
                .sort((a, b) => a.id - b.id);
            this.availableServices = this.services
                .filter(s => this._originalBountServices.indexOf(s.id) === -1)
                .sort((a, b) => a.id - b.id);
        }
    }
}
