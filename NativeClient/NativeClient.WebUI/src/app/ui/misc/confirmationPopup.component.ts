import { Component, Output, EventEmitter, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-confirmation-popup',
    templateUrl: './confirmationPopup.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmationPopupComponent {
    @Output() private answerEvent = new EventEmitter<boolean>();

    @Input() private display = false;

    public get messageVisible(): boolean {
        return this.display;
    }

    public answerYes() {
        this.answerEvent.emit(true);
    }

    public answerNo() {
        this.answerEvent.emit(false);
    }
}
