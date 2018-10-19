import { Component, Input } from '@angular/core';
import { BackendErrorStatuses } from '../../model/httpModel/backendErrorStatuses.model';

@Component({
    selector: 'app-backend-error-selector',
    templateUrl: './backendErrorSelector.component.html'
})
export class BackendErrorSelectorComponent {
    errorStatus = BackendErrorStatuses;
    @Input() message: BackendErrorStatuses;
}
