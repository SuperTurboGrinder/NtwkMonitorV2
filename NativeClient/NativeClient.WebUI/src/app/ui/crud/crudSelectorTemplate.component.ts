import { Component, Input, Output, EventEmitter } from "@angular/core";

@Component({
    selector: 'crudSelectorTemplate',
    templateUrl: './crudSelectorTemplate.component.html'
})
export class CrudSelectorTemplateComponent {

    @Input() isCustomView = false;

    @Input() baseFormLink = "";
    @Input() dataList: any[] = null;

    @Input() dataLoaded = false;
    @Input() displayOperationInProgress = false;
    @Input() isLoadingError = false;
    
    private objectToRemoveID: number;
    private objectToRemoveName = ""
    private displayDeleteMessage = false;

    @Output() private refreshEvent = new EventEmitter<boolean>();
    @Output() private deleteAnswerEvent =
        new EventEmitter<{shouldDelete:boolean, id:number}>();

    public refresh(_: boolean) {
        this.refreshEvent.emit(_);
    }

    public deleteHandler(shouldDelete: boolean) {
        this.displayDeleteMessage = false;
        this.deleteAnswerEvent
            .emit({shouldDelete:shouldDelete, id:this.objectToRemoveID});
    }

    public tryRemove(objectToRemove: any) {
        this.objectToRemoveID = objectToRemove.id;
        this.objectToRemoveName = objectToRemove.name;
        this.displayDeleteMessage = true;
    }
}