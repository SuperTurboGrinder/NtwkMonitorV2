import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
    selector: 'app-loading-indicator',
    templateUrl: './loadingIndicator.component.html'
})
export class LoadingIndicatorComponent {
    @Output() refreshEvent = new EventEmitter<boolean>();
    @Input() isError = false;

    refresh() {
        this.refreshEvent.emit(true);
    }
}
