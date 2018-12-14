import { Component, AfterViewInit, HostListener, Input, ContentChild, TemplateRef, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-popup-panel-container',
    templateUrl: './popupPanelContainer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PopupPanelContainerComponent implements AfterViewInit {
    private screenWH = ({width: 800, height: 600});
    private readonly emptyStyles: {[k: string]: string} = {
        'bottom': '0px',
        'left': '0px'
    };
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

    get positionStyles(): {[k: string]: string} {
        if (this.screenPos === null) {
            return this.emptyStyles;
        }
        const styles: {[k: string]: string} = {};
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
