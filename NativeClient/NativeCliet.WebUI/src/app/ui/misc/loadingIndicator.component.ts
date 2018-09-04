import { Component, EventEmitter, Input, Output } from "@angular/core";

@Component({
    selector: 'loadingIndicator',
    templateUrl: './loadingIndicator.component.html'
})
export class LoadingIndicatorComponent {
    @Output() refreshEvent = new EventEmitter<boolean>();
    @Input() isError: boolean = false;

    refresh() {
        this.refreshEvent.emit(true);
    }
}