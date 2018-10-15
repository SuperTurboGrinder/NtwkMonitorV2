import { Component, Input, Output, EventEmitter  } from "@angular/core";

@Component({
    selector: 'crudFormScaffolding',
    templateUrl: './crudFormScaffolding.component.html'
})
export class CrudFormScaffoldingComponent {
    @Input() set displayOperationInProgress(val: boolean) {
        console.log(`Swapping ${this._displayOperationInProgress} with ${val}`);
        this._displayOperationInProgress = val;
    }
    get displayOperationInProgress() {
        return this._displayOperationInProgress;
    }
    _displayOperationInProgress = false;
    @Input() displayDiscardMessage = false;
    @Input() displayCreateConfirmationMessage = false;
    @Input() displaySaveChangesMessage = false;
    @Input() isLoadingError = false;

    @Output() private discardConfirmationEvent = new EventEmitter<boolean>();
    @Output() private saveNewConfirmationEvent = new EventEmitter<boolean>();
    @Output() private saveChangesConfirmationEvent = new EventEmitter<boolean>();
    @Output() private refreshEvent = new EventEmitter<boolean>();

    public discardAndReturn(shouldDiscard: boolean) {
        this.discardConfirmationEvent.emit(shouldDiscard);
    }

    createNewObjectAndReturn(shouldSave: boolean) {
        this.saveNewConfirmationEvent.emit(shouldSave);
    }

    saveChangesToObjectAndReturn(shouldSave: boolean) {
        this.saveChangesConfirmationEvent.emit(shouldSave);
    }

    refresh(_: boolean) {
        this.refreshEvent.emit(_);
    }
}