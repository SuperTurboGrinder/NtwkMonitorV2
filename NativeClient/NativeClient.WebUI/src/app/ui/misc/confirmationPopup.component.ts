import { Component, Output, EventEmitter, Input } from "@angular/core";

@Component({
    selector: 'confirmationPopup',
    templateUrl: './confirmationPopup.component.html'
})
export class ConfirmationPopupComponent {
    @Output() private answerEvent = new EventEmitter<boolean>();

    @Input() private display: boolean = false;

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