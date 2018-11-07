import { Component, OnDestroy, AfterViewInit, HostListener } from '@angular/core';

import { NodeInfoPopupData } from '../../model/viewModel/nodeInfoPopupData.model';
import { Subscription } from 'rxjs';
import { NodeInfoPopupDataService } from '../../services/nodeInfoPopupData.service';

@Component({
    selector: 'app-node-info-popup',
    templateUrl: './nodeInfoPopup.component.html'
})
export class NodeInfoPopupComponent implements AfterViewInit {
    private data: NodeInfoPopupData = null;
    private screenWH = ({width: 800, height: 600});
    private nodeInfoPopupDataSubscription: Subscription;

    nodeName(): string { return this.data.node.name; }
    nodeIP(): string { return this.data.node.ipStr; }
    tagsNames(): string[] { return this.data.tagsNames; }
    webServicesNames(): string[] { return this.data.webServicesNames; }

    constructor(
        nodeInfoPopupDataService: NodeInfoPopupDataService
    ) {
        this.nodeInfoPopupDataSubscription = nodeInfoPopupDataService
            .subscribeToData(data => this.data = data);
    }

    ngAfterViewInit() {
        this.onResize();
    }

    @HostListener('window:resize', ['$event'])
    onResize() {
        this.screenWH.width = document.body.clientWidth;
        this.screenWH.height = document.body.clientHeight;
    }

    get positionStyles(): any {
        const styles: {[k: string]: any} = {};
        const isLeft = this.data.screenPos.x < this.screenWH.width / 2;
        const isTop = this.data.screenPos.y < this.screenWH.height / 2;
        const shift = { x: isLeft ? +3 : -3, y: isTop ? +3 : -3 };
        const posX = !isLeft
            ? this.screenWH.width - this.data.screenPos.x
            : this.data.screenPos.x;
        const posY = !isTop
            ? this.screenWH.height - this.data.screenPos.y
            : this.data.screenPos.y;
        styles[isTop ? 'top' : 'bottom']
            = `${posY + shift.y}px`;
        styles[isLeft ? 'left' : 'right']
            = `${posX + shift.x}px`;
        return styles;
    }

    public execServicesList(): string {
        const services: string[] = [];
        if (this.data.node.isOpenTelnet === true) {
            services.push('Telnet');
        }
        if (this.data.node.isOpenSSH === true) {
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

    public hasWebServices(): boolean {
        return this.data.webServicesNames.length !== 0;
    }

    public hasTags(): boolean {
        return this.data.tagsNames.length !== 0;
    }
}
