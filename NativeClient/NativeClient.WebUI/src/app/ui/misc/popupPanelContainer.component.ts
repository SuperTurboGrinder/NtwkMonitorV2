import { Component, AfterViewInit, HostListener, Input, ContentChild, TemplateRef } from '@angular/core';

@Component({
    selector: 'app-popup-panel-container',
    templateUrl: './popupPanelContainer.component.html'
})
export class PopupPanelContainerComponent implements AfterViewInit {
    private screenWH = ({width: 800, height: 600});
    @Input() screenPos: { x: number, y: number } = null;
    @ContentChild(TemplateRef) contentRef;

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
        const isLeft = this.screenPos.x < this.screenWH.width / 2;
        const isTop = this.screenPos.y < this.screenWH.height / 2;
        const offset = 10;
        const posX = !isLeft
            ? this.screenWH.width - this.screenPos.x
            : this.screenPos.x;
        const posY = !isTop
            ? this.screenWH.height - this.screenPos.y
            : this.screenPos.y;
        styles[isTop ? 'top' : 'bottom']
            = `${posY + offset}px`;
        styles[isLeft ? 'left' : 'right']
            = `${posX + offset}px`;
        return styles;
    }
}
