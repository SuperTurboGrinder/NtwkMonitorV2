import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-loading-indicator',
    templateUrl: './loadingIndicator.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingIndicatorComponent {
    @Output() refreshEvent = new EventEmitter<boolean>();
    @Input() isError = false;

    refresh() {
        this.refreshEvent.emit(true);
    }
}
