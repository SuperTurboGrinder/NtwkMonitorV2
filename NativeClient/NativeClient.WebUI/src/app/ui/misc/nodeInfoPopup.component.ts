import { Component, OnDestroy } from '@angular/core';

import { NodeInfoPopupData } from '../../model/viewModel/nodeInfoPopupData.model';
import { Subscription } from 'rxjs';
import { NodeInfoPopupDataService } from '../../services/nodeInfoPopupData.service';

@Component({
    selector: 'app-node-info-popup',
    templateUrl: './nodeInfoPopup.component.html'
})
export class NodeInfoPopupComponent implements OnDestroy {
    private data: NodeInfoPopupData = null;
    private nodeInfoPopupDataSubscription: Subscription;

    nodeName(): string {
        return this.data === null ? '' : this.data.node.name;
    }
    nodeIP(): string {
        return this.data === null ? '' : this.data.node.ipStr;
    }
    tagsNames(): string[] {
        return this.data === null ? [] : this.data.tagsNames;
    }
    webServicesNames(): string[] {
        return this.data === null ? [] : this.data.webServicesNames;
    }
    screenPos(): { x: number, y: number } {
        return this.data === null ? null : this.data.screenPos;
    }

    constructor(
        nodeInfoPopupDataService: NodeInfoPopupDataService
    ) {
        this.nodeInfoPopupDataSubscription = nodeInfoPopupDataService
            .subscribeToData(data => this.data = data);
    }

    ngOnDestroy() {
        this.nodeInfoPopupDataSubscription.unsubscribe();
    }

    public execServicesList(): string {
        const services: string[] = [];
        if (this.data.node.isOpenTelnet === true) {
            services.push('Telnet');
        }
        if (this.data.node.isOpenSsh === true) {
            services.push('SSH');
        }
        if (this.data.node.isOpenPing === true) {
            services.push('Ping');
        }
        if (services.length === 0) {
            return 'None.';
        }
        return services.reduce((prev, current) =>
            `${prev}${prev.length === 0 ? '' : ', '}${current}`
        ) + '.';
    }

    public showExecServices(): boolean {
        return this.data === null ? false : this.data.showExecServices;
    }

    public showWebServices(): boolean {
        return this.data === null ? false : this.data.showWebServices;
    }

    public showTags(): boolean {
        return this.data === null ? false : this.data.showTags;
    }

    public hasWebServices(): boolean {
        return this.data === null ? false : this.data.webServicesNames.length !== 0;
    }

    public hasTags(): boolean {
        return this.data === null ? false : this.data.tagsNames.length !== 0;
    }
}
