import { Component, EventEmitter, Input } from "@angular/core";

import { NtwkNode } from "../../model/httpModel/ntwkNode.model";

@Component({
    selector: 'nodeInfoPopup',
    templateUrl: './nodeInfoPopup.component.html'
})
export class NodeInfoPopupComponent {
    @Input() node: NtwkNode = null;
    @Input() webServicesNames: {name:string,serviceID:number}[] = null;
    @Input() tagsNames: string[] = null;
    @Input() screenData: { x:number, y:number, mouseX:number, mouseY:number }

    get positionStyles() : any {
        let styles: {[k: string]: any} = {};
        let isTop = this.screenData.mouseY < this.screenData.y/2;
        let isLeft = this.screenData.mouseX < this.screenData.x/2;
        let shift = { x: isLeft ? +3 : -3, y: isTop ? +3 : -3 }
        styles[isTop ? "top" : "bottom"]
            = `${this.screenData.mouseY + shift.y}px`;
        styles[isLeft ? "left" : "right"]
            = `${this.screenData.mouseX + shift.x}px`;
        return styles;
    }

    public execServicesList() : string {
        let services: string[] = [];
        if(this.node.isOpenTelnet === true) services.push("Telnet");
        if(this.node.isOpenSSH === true) services.push("SSH");
        if(this.node.isOpenPing === true) services.push("Ping");
        if(services.length === 0) return "None.";
        return services.reduce((prev, current) => 
            `${prev}${prev.length===0 ? '' : ', '}${current}`
        )+".";
    }

    public hasWebServices() : boolean {
        return this.webServicesNames != null
            && this.webServicesNames.length != 0;
    }

    public hasTags() : boolean {
        return this.tagsNames != null
            && this.tagsNames.length != 0;
    }
}