import {
    Component,
    Input, Output, EventEmitter,
    ContentChild, TemplateRef
} from '@angular/core';
import { NodeLineData } from 'src/app/model/viewModel/nodeLineData.model';

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

    refresh(_: boolean) {
        this.refreshEvent.emit(_);
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
}
