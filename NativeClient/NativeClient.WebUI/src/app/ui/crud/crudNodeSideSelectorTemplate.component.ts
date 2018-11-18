import {
    Component,
    Input, Output, EventEmitter,
    ContentChild, TemplateRef
} from '@angular/core';
import { NodeLineData } from 'src/app/model/viewModel/nodeLineData.model';
import { Location } from '@angular/common';

@Component({
    selector: 'app-crud-node-side-selector-template',
    templateUrl: './crudNodeSideSelectorTemplate.component.html'
})
export class CrudNodeSideSelectorTemplateComponent {
    private selectedNodeLine: NodeLineData = null;
    // in place of ng-content
    // fixes rendering before initialization
    @ContentChild(TemplateRef) contentRef;
    @Input() dataList: NodeLineData[] = null;
    @Input() isLoadingError = false;
    @Input() displayOperationInProgress = false;
    @Output() private refreshEvent = new EventEmitter<boolean>();
    @Output() private nodeSelectionEvent = new EventEmitter<number>();
    @Output() private showInfoEvent = new EventEmitter<{
        lineData: NodeLineData
        mousePos: { x: number, y: number }
    }>();
    @Output() private hideInfoEvent = new EventEmitter();

    constructor(
        private locationService: Location
    ) {}

    refresh(_: boolean) {
        this.refreshEvent.emit(_);
    }

    public returnToPreviousLocation() {
        this.locationService.back();
    }

    public isSelectedNode(nodeID: number) {
        return this.selectedNodeLine !== null
            ? this.selectedNodeLine.id === nodeID
            : false;
    }

    public selectNode(node: NodeLineData) {
        this.selectedNodeLine = node;
        this.nodeSelectionEvent.emit(node.id);
    }

    public showInfo(lineData: NodeLineData, event: MouseEvent) {
        this.showInfoEvent.emit({
            lineData: lineData,
            mousePos: {x: event.clientX, y: event.clientY}
        });
    }

    public hideInfo() {
        this.hideInfoEvent.emit();
    }
}
