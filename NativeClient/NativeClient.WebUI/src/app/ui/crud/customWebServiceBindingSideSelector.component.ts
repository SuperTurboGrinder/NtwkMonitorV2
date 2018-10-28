import { Component } from '@angular/core';
import { NodesService } from '../../services/nodes.service';
import { MessagingService } from 'src/app/services/messaging.service';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';
import { BaseCrudNodeSideSelectorComponent } from '../helpers/baseCrudNodeSideSelectorComponent.helper';
import { CustomWebServicesService } from 'src/app/services/customWebServices.service';
import { CustomWebService } from 'src/app/model/httpModel/customWebService.model';
import { HTTPResult } from 'src/app/model/servicesModel/httpResult.model';
import { CustomWebServiceBinding } from 'src/app/model/httpModel/cwsBindingData.model';

@Component({
    selector: 'app-custom-web-service-binding-side-selector',
    templateUrl: './customWebServiceBindingSideSelector.component.html'
})
export class CustomWebServiceBindingSideSelectorComponent
    extends BaseCrudNodeSideSelectorComponent {

    allServices: CustomWebService[] = null;


    constructor(
        messager: MessagingService,
        dataService: NodesService,
        private cwsService: CustomWebServicesService
    ) {
        super(messager, dataService);
        this.updateServicesList();
    }

    public get selectedNodeServicesData(): {
        nodeID: number
        servicesIDs: number[]
    } {
        return this.selectedNodeData !== null
            ? {
                nodeID: this.selectedNodeData.node.id,
                servicesIDs: this.selectedNodeData.boundWebServicesIDs
            }
            : null;
    }

    private updateServicesList() {
        this.cwsService.getCWSList().subscribe(
            (servicesResult: HTTPResult<CustomWebService[]>) => {
                if (servicesResult.success === true) {
                    this.allServices = servicesResult.data;
                }
            }
        );
    }

    public bindService(binding: {
        serviceID: number,
        bindingData: CustomWebServiceBinding
    }) {
        this.displayOperationInProgress = true;
        this.cwsService.createNewCWSBinding(
            binding.serviceID,
            binding.bindingData,
            success => this.confirmOperationSuccess(
                success,
                MessagesEnum.CreatedSuccessfully
            )
        );
    }

    public removeBinding(serviceID: number) {
        const nodeID = this.selectedNodeData.node.id;
        this.displayOperationInProgress = true;
        this.cwsService.deleteCWSBinding(
            serviceID,
            nodeID,
            success => this.confirmOperationSuccess(
                success,
                MessagesEnum.DeletedSuccessfully
            )
        );
    }
}
