import { Component, EventEmitter, Input } from "@angular/core";

import { NodeInfoPopupData } from "../../model/viewModel/nodeInfoPopupData.model";
import { ScreenSizeService } from "../../services/screenSize.service";
import { Subscription } from "rxjs";
import { NodeInfoPopupDataService } from "../../services/nodeInfoPopupData.service";

@Component({
    selector: 'nodeInfoPopup',
    templateUrl: './nodeInfoPopup.component.html'
})
export class NodeInfoPopupComponent {
    private data: NodeInfoPopupData = null;
    private screenWH: {width: number, height: number}
    private screenSizeSubscription: Subscription;
    private nodeInfoPopupDataSubscription: Subscription;

    nodeName(): string { return this.data.node.name; }
    nodeIP(): string { return this.data.node.ipStr; }
    tagsNames(): string[] { return this.data.tagsNames; }
    webServicesNames(): string[] { return this.data.webServicesNames; }

    constructor(
        screenSizeService: ScreenSizeService,
        nodeInfoPopupDataService: NodeInfoPopupDataService
    ) {
        this.screenSizeSubscription = screenSizeService
            .subscribeToScreenSize(scrSize => this.screenWH = scrSize);
        this.nodeInfoPopupDataSubscription = nodeInfoPopupDataService
            .subscribeToData(data => this.data = data);
    }

    ngOnDestroy() {
        this.screenSizeSubscription.unsubscribe();
        this.nodeInfoPopupDataSubscription.unsubscribe();
    }

    get positionStyles() : any {
        let styles: {[k: string]: any} = {};
        let isLeft = this.data.screenPos.x < this.screenWH.width/2;
        let isTop = this.data.screenPos.y < this.screenWH.height/2;
        let shift = { x: isLeft ? +3 : -3, y: isTop ? +3 : -3 }
        styles[isTop ? "top" : "bottom"]
            = `${this.data.screenPos.y + shift.y}px`;
        styles[isLeft ? "left" : "right"]
            = `${this.data.screenPos.x + shift.x}px`;
        return styles;
    }

    public execServicesList() : string {
        let services: string[] = [];
        if(this.data.node.isOpenTelnet === true) services.push("Telnet");
        if(this.data.node.isOpenSSH === true) services.push("SSH");
        if(this.data.node.isOpenPing === true) services.push("Ping");
        if(services.length === 0) return "None.";
        return services.reduce((prev, current) => 
            `${prev}${prev.length===0 ? '' : ', '}${current}`
        )+".";
    }

    public hasWebServices() : boolean {
        return this.data.webServicesNames.length != 0;
    }

    public hasTags() : boolean {
        return this.data.tagsNames.length != 0;
    }
}